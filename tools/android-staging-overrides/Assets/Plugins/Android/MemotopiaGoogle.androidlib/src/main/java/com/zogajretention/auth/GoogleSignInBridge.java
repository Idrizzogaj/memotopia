package com.zogajretention.auth;

import android.app.Activity;
import android.os.CancellationSignal;
import androidx.credentials.Credential;
import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;
import androidx.credentials.CustomCredential;
import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialException;
import androidx.credentials.exceptions.GetCredentialCancellationException;
import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;
import java.util.concurrent.Executor;

public final class GoogleSignInBridge {
    public interface Callback {
        void onSuccess(String idToken);
        void onError(String code);
    }
    private static CancellationSignal pending;
    public static void cancel() {
        if (pending != null) pending.cancel();
        pending = null;
    }
    public static void signIn(final Activity activity, final String clientId, final Callback callback) {
        activity.runOnUiThread(() -> {
            cancel();
            final CancellationSignal requestSignal = new CancellationSignal();
            pending = requestSignal;
            try {
                GetSignInWithGoogleOption option = new GetSignInWithGoogleOption.Builder(clientId).build();
                GetCredentialRequest request = new GetCredentialRequest.Builder().addCredentialOption(option).build();
                Executor main = runnable -> activity.runOnUiThread(runnable);
                CredentialManager.create(activity).getCredentialAsync(activity, request, requestSignal, main,
                    new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>() {
                        @Override public void onResult(GetCredentialResponse result) {
                            if (pending != requestSignal) return;
                            pending = null;
                            try {
                                Credential credential = result.getCredential();
                                if (!(credential instanceof CustomCredential) || !GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL.equals(credential.getType())) {
                                    callback.onError("invalid_credential"); return;
                                }
                                callback.onSuccess(GoogleIdTokenCredential.createFrom(credential.getData()).getIdToken());
                            } catch (Exception ignored) { callback.onError("invalid_credential"); }
                        }
                        @Override public void onError(GetCredentialException error) {
                            if (pending != requestSignal) return;
                            pending = null;
                            callback.onError(error instanceof GetCredentialCancellationException ? "cancelled" : "unavailable");
                        }
                    });
            } catch (Exception ignored) { pending = null; callback.onError("unavailable"); }
        });
    }
}

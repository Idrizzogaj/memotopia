#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>

#if TARGET_OS_TV || TARGET_OS_IOS
#import "UnityAppController.h"
#endif

struct UserInfo
{
    const char * userId;
    const char * email;
    const char * displayName;

    const char * idToken;
    const char * error;

    ASUserDetectionStatus userDetectionStatus;
};

typedef void (*SignInWithAppleCallback)(int result, struct UserInfo s1);
typedef void (*CredentialStateCallback)(ASAuthorizationAppleIDProviderCredentialState state);

@interface UnitySignInWithApple : NSObject<ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>

@property (nonatomic) SignInWithAppleCallback loginCallback;
@property (nonatomic, strong) ASAuthorizationController* activeController;
@property (nonatomic) CredentialStateCallback credentialStateCallback;

@end

static UnitySignInWithApple* _unitySignInWithAppleInstance;

@implementation UnitySignInWithApple
{
    ASAuthorizationAppleIDRequest* request;
}

+(UnitySignInWithApple*)instance
{
    if (_unitySignInWithAppleInstance == nil)
        _unitySignInWithAppleInstance = [[UnitySignInWithApple alloc] init];
    return _unitySignInWithAppleInstance;
}

-(void)startRequest
{
    if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *)) {
        ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
        request = [provider createRequest];
        [request setRequestedScopes: @[ASAuthorizationScopeEmail, ASAuthorizationScopeFullName]];

        self.activeController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
        ASAuthorizationController* controller = self.activeController;
        controller.delegate = self;
        controller.presentationContextProvider = self;
        [controller performRequests];
    } else {
        // Fallback on earlier versions
    }
}

- (void)getCredentialState:(NSString *)userID
{
    ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
    [provider getCredentialStateForUserID:userID
                               completion:^(ASAuthorizationAppleIDProviderCredentialState credentialState, NSError * _Nullable error) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.credentialStateCallback(credentialState);
        });
    }];
}

-(ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller
{
#if TARGET_OS_TV || TARGET_OS_IOS
    UIWindow* window = _UnityAppController.window;
    for (UIScene* scene in UIApplication.sharedApplication.connectedScenes) {
        if (scene.activationState == UISceneActivationStateForegroundActive && [scene isKindOfClass:[UIWindowScene class]]) {
            for (UIWindow* candidate in ((UIWindowScene*)scene).windows) {
                if (candidate.isKeyWindow) return candidate;
            }
        }
    }
    return window;
#else
    return [[NSApplication sharedApplication] mainWindow];
#endif
}

-(void)authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization
{
    if (self.loginCallback)
    {
        struct UserInfo data = {0};

        if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *)) {
            ASAuthorizationAppleIDCredential* credential = (ASAuthorizationAppleIDCredential*)authorization.credential;
            NSString* idToken = [[NSString alloc] initWithData:credential.identityToken encoding:NSUTF8StringEncoding];
            NSPersonNameComponents* name = credential.fullName;

            data.idToken = [idToken UTF8String];

            data.displayName = name == nil ? "" : [[NSPersonNameComponentsFormatter localizedStringFromPersonNameComponents:name
                                                                                                   style:NSPersonNameComponentsFormatterStyleDefault
                                                                                                 options:0] UTF8String];
            // Email gets treated a little differently. Rather than an empty string it will return nil for no value.
            data.email = credential.email == nil ? "" : [credential.email UTF8String];
            data.userId = [credential.user UTF8String];
            data.userDetectionStatus = credential.realUserStatus;
            data.error = "";
            self.loginCallback(1, data);
            self.loginCallback = NULL;
            self.activeController = nil;
        } else {
            // Fallback on earlier versions
        }
    }
}

-(void)authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error
{
    if (self.loginCallback)
    {
        // All members need to be set to a non-null value.
        struct UserInfo data = {0};
        data.idToken = "";
        data.displayName = "";
        data.email = "";
        data.userId = "";
        data.userDetectionStatus = 1;
        NSString* message = [NSString stringWithFormat:@"%@ (Apple error %ld)", error.localizedDescription, (long)error.code];
        data.error = [message UTF8String];
        self.loginCallback(0, data);
        self.loginCallback = NULL;
        self.activeController = nil;
    }
}

@end

void UnitySignInWithApple_Login(SignInWithAppleCallback callback)
{
    if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *)) {
        UnitySignInWithApple* login = [UnitySignInWithApple instance];
        login.loginCallback = callback;
        dispatch_async(dispatch_get_main_queue(), ^{ [login startRequest]; });
    } else {
        // Fallback on earlier versions
    }
}

void UnitySignInWithApple_GetCredentialState(const char *userID, CredentialStateCallback callback)
{
    if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *)) {
        UnitySignInWithApple* login = [UnitySignInWithApple instance];
        login.credentialStateCallback = callback;
        [login getCredentialState: [NSString stringWithUTF8String: userID]];
    } else {
        // Fallback on earlier versions
    }
}

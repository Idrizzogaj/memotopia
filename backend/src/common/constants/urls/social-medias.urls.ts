export const socialMediasReqUrls = {
    appleAuthKeys: 'https://appleid.apple.com/auth/keys',
    facebook: (userId: string, accessToken: string): string => {
        return `https://graph.facebook.com/${userId}?fields=name,email&access_token=${accessToken}`;
    },
};

#import <UIKit/UIKit.h>

// Opens a web or store link with the current UIKit API. Unity's own Application.OpenURL uses the old call, which
// recent iOS versions can refuse without any message.
void Memotopia_OpenURL(const char* url)
{
    if (url == NULL) return;
    NSString* text = [NSString stringWithUTF8String:url];
    NSURL* target = [NSURL URLWithString:text];
    if (target == nil) return;
    dispatch_async(dispatch_get_main_queue(), ^{
        [[UIApplication sharedApplication] openURL:target options:@{} completionHandler:^(BOOL success) {
            NSLog(@"Memotopia_OpenURL %@ success=%d", text, success);
        }];
    });
}
#import <UIKit/UIKit.h>

extern UIViewController* UnityGetGLViewController(void);

// Opens the iOS share sheet with plain text.
extern "C" void _MemotopiaShareText(const char* text)
{
    NSString* message = [NSString stringWithUTF8String:text ? text : ""];
    UIActivityViewController* sheet = [[UIActivityViewController alloc] initWithActivityItems:@[message] applicationActivities:nil];
    UIViewController* root = UnityGetGLViewController();
    if (sheet.popoverPresentationController != nil)
    {
        // iPad requires an anchor for the popover.
        sheet.popoverPresentationController.sourceView = root.view;
        sheet.popoverPresentationController.sourceRect = CGRectMake(root.view.bounds.size.width / 2.0, root.view.bounds.size.height / 2.0, 1, 1);
        sheet.popoverPresentationController.permittedArrowDirections = (UIPopoverArrowDirection)0;
    }
    [root presentViewController:sheet animated:YES completion:nil];
}

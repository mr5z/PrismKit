namespace PrismKit.ViewModels;

public interface IDismissableModal
{
    bool DismissOnBackgroundClick();
    bool DismissOnBackButtonPress();
}

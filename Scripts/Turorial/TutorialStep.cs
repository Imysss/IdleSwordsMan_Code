using Data;
using UnityEngine;

public abstract class TutorialStep
{
    protected TutorialStepData Data;
        
    public virtual void ShowUsingData(TutorialStepData data)
    {
        Data = data;
        ConfigureView();
    }

    /// <summary>
    /// ShowUsingData 메서드를 통해 할당된 TutorialPageData를 이용해 화면을 구성하게 만드는 메서드.
    /// </summary>
    protected abstract void ConfigureView();
}

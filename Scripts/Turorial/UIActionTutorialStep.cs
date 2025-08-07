using TMPro;
using UnityEngine;
using Coffee.UIExtensions;
using Excellcube.EasyTutorial.Widget;
using UnityEngine.UI;

public class UIActionTutorialStep : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_ActionLogText;
    public  TextMeshProUGUI ActionLogText => m_ActionLogText;

    [SerializeField] 
    private Image _actionImage;
    public Image ActionImage => _actionImage;
    

    [SerializeField]
    private Indicator m_Indicator;
    public  Indicator Indicator => m_Indicator;
    
    [SerializeField]
    private Unmask m_UnmaskPanel;
    public  Unmask UnmaskPanel => m_UnmaskPanel;
}

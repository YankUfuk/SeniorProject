using TMPro;
using UnityEngine;

public class EducationalInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;

    public void ShowSusceptibleInfo()
    {
        SetInfo(EducationalInfoProvider.GetSusceptibleExplanation());
    }

    public void ShowExposedInfo()
    {
        SetInfo(EducationalInfoProvider.GetExposedExplanation());
    }

    public void ShowInfectedInfo()
    {
        SetInfo(EducationalInfoProvider.GetInfectedExplanation());
    }

    public void ShowRecoveredInfo()
    {
        SetInfo(EducationalInfoProvider.GetRecoveredExplanation());
    }

    public void ShowLockdownInfo()
    {
        SetInfo(EducationalInfoProvider.GetLockdownExplanation());
    }

    public void ShowVaccinationInfo()
    {
        SetInfo(EducationalInfoProvider.GetVaccinationExplanation());
    }

    public void ShowHospitalCapacityInfo()
    {
        SetInfo(EducationalInfoProvider.GetHospitalCapacityExplanation());
    }

    public void ShowBudgetInfo()
    {
        SetInfo(EducationalInfoProvider.GetBudgetExplanation());
    }

    public void ShowSEIRInfo()
    {
        SetInfo(EducationalInfoProvider.GetSEIRExplanation());
    }

    public void ClearInfo()
    {
        SetInfo(string.Empty);
    }

    private void SetInfo(string message)
    {
        if (infoText == null)
        {
            Debug.LogWarning("EducationalInfoUI: infoText is not assigned.");
            return;
        }

        infoText.text = message;
    }
}

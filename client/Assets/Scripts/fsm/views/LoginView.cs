using UnityEngine.UI;
using TMPro;
using UnityEngine;

/**
 * Wraps all elements and functionality required for the LoginView.
 */
public class LoginView : View
{
    [SerializeField] private TMP_InputField inputFieldName = null;
    [SerializeField] private TMP_InputField inputFieldIP = null;
    [SerializeField] private Button buttonConnect = null;
    [SerializeField] private TextMeshProUGUI textConnectResults = null;
    [SerializeField] private GameObject goConnectResultsRow = null;

    public string userName { get => inputFieldName.text; set => inputFieldName.text = value; }
    public string IP { get => inputFieldIP.text; set => inputFieldIP.text = value; }
    public Button ButtonConnect { get => buttonConnect; }

    public string TextConnectResults
    {
        set
        {
            if (value.Length > 601)
                textConnectResults.text = value.Substring(0, 600) + "...";
            else
                textConnectResults.text = value;
            //enable the row with status info based on whether we actually have status info
            goConnectResultsRow.SetActive(value != null && value.Length > 0);
            textConnectResults.ForceMeshUpdate();
            goConnectResultsRow.GetComponent<LayoutElement>().minHeight = textConnectResults.renderedHeight + 10;
        }
    }
}
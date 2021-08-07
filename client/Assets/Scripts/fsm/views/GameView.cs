using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * Wraps all elements and functionality required for the GameView.
 */
public class GameView : View
{
    [SerializeField] private ScoreBoard _scoreboard = null;
    public ScoreBoard scoreboard => _scoreboard;

    [SerializeField] private Button _buttonRoll = null;
    public Button buttonRoll => _buttonRoll;

    [SerializeField] private Button _buttonEnd = null;
    public Button buttonEnd => _buttonEnd;

    [SerializeField] private Button _buttonLeave = null;
    public Button buttonLeave => _buttonLeave;

    [SerializeField] private TextMeshProUGUI _timeUI = null;
    public TextMeshProUGUI timeUI => _timeUI;

    [SerializeField] private TextMeshPro _itemsLeftText = null;
    public TextMeshPro itemsLeftText => _itemsLeftText;
    
    [SerializeField] private TextMeshProUGUI _infoText = null;
    public TextMeshProUGUI infoText => _infoText;

    [SerializeField] private Transform[] _slots = null;
    public Transform[] slots => _slots;
    
    [SerializeField] private GameObject _storageParent = null;
    public GameObject storageParent => _storageParent;

    [SerializeField] private GameViewEndOverlay _endOverlay = null;
    public GameViewEndOverlay endOverlay => _endOverlay;
    
    [SerializeField] private GameObject _dicePrefab = null;
    public GameObject dicePrefab => _dicePrefab;
}


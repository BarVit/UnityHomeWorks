using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapScreen : MonoBehaviour
{
    private const string GalaxyTitle = "MILKY WAY";
    private const string ArmTitle = "ORION ARM";
    private const string SystemTitle = "SYSTEM ";
    private const string DemoOnly = "NOT AVAILABLE IN DEMO";
    private const string PlayText = "PLAY";
    private const string ReplayText = "REPLAY";
    private const string NoSelection = null;
    private const float BranchDistance = 230f;
    private const float ImageWidth = 1920f;
    private const float RouteWidth = 4f;
    private const float Dash = 14f;
    private const float Gap = 10f;
    private const float DashSpeed = 40f;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Economy _economy;
    [SerializeField] private LocationTable _locations;
    [SerializeField] private WaveTable _waves;
    [SerializeField] private SystemTable _systems;
    [SerializeField] private string _testLocationId = "loc_test";
    [SerializeField] private GameObject _galaxyView;
    [SerializeField] private GameObject _armView;
    [SerializeField] private GameObject _systemView;
    [SerializeField] private MapRegion[] _regions;
    [SerializeField] private MapOverlay _galaxyOverlay;
    [SerializeField] private MapNode[] _systemNodes;
    [SerializeField] private Vector2[] _systemPositions;
    [SerializeField] private UiPolyline _armDone;
    [SerializeField] private UiPolyline _armRest;
    [SerializeField] private Image _systemBackground;
    [SerializeField] private Sprite[] _systemSprites;
    [SerializeField] private Color[] _starColors;
    [SerializeField] private MapNode[] _locationNodes;
    [SerializeField] private UiPolyline _routeDone;
    [SerializeField] private UiPolyline _routeRest;
    [SerializeField] private LocationPositions[] _locationPositions;
    [SerializeField] private MapNode[] _branchNodes;
    [SerializeField] private UiPolyline[] _branchLinks;
    [SerializeField] private Vector2 _starCentre = new Vector2(0.5f, 0.4907f);
    [SerializeField] private Color _branchColor = new Color(0.7f, 0.62f, 1f);
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _info;
    [SerializeField] private Button _backButton;
    [SerializeField] private MapToast _toast;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _dockButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private TMP_Text _playLabel;
    [SerializeField] private Color _doneRouteColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color _restRouteColor = new Color(0.6f, 0.7f, 0.85f, 0.35f);

    private readonly List<LocationRow> _route = new List<LocationRow>();
    private readonly List<SystemRow> _systemRoute = new List<SystemRow>();
    private readonly List<LocationRow> _shownBranches = new List<LocationRow>();
    private int _currentLocation;
    private int _openedSystem = -1;
    private string _selected = NoSelection;
    private GameObject _lastView;
    private bool _canPlay;

    public event Action MenuRequested;
    public event Action DockRequested;
    public event Action PlayRequested;
    public event Action<string> LocationPlayRequested;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(Back);
        _menuButton.onClick.AddListener(OnMenuClicked);
        _dockButton.onClick.AddListener(OnDockClicked);
        _playButton.onClick.AddListener(OnPlayClicked);
        _economy.SessionStarted += ForgetView;

        foreach (MapRegion region in _regions)
        {
            region.Clicked += OnRegionClicked;
            region.Hovered += OnRegionHovered;
        }

        foreach (MapNode node in _systemNodes)
            node.Clicked += OnSystemClicked;

        foreach (MapNode node in _locationNodes)
            node.Clicked += OnLocationClicked;

        foreach (MapNode node in _branchNodes)
            node.Clicked += OnBranchClicked;
    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(Back);
        _menuButton.onClick.RemoveListener(OnMenuClicked);
        _dockButton.onClick.RemoveListener(OnDockClicked);
        _playButton.onClick.RemoveListener(OnPlayClicked);
        _economy.SessionStarted -= ForgetView;

        foreach (MapRegion region in _regions)
        {
            region.Clicked -= OnRegionClicked;
            region.Hovered -= OnRegionHovered;
        }

        foreach (MapNode node in _systemNodes)
            node.Clicked -= OnSystemClicked;

        foreach (MapNode node in _locationNodes)
            node.Clicked -= OnLocationClicked;

        foreach (MapNode node in _branchNodes)
            node.Clicked -= OnBranchClicked;
    }

    public void Show(bool canPlay)
    {
        _canPlay = canPlay;
        Select(NoSelection);
        _panel.transform.SetAsLastSibling();
        _panel.SetActive(true);
        ReadProgress();

        if (_lastView == _galaxyView)
            ShowGalaxy();
        else if (_lastView == _armView)
            ShowArm();
        else
            ShowSystem(_lastView == _systemView ? _openedSystem : GetCurrentSystem());
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void ReadProgress()
    {
        _systemRoute.Clear();
        _route.Clear();

        foreach (SystemRow system in _systems.Rows)
            _systemRoute.Add(system);

        _systemRoute.Sort((left, right) => left.Order.CompareTo(right.Order));

        foreach (SystemRow system in _systemRoute)
            _route.AddRange(GetLocations(system.Id));

        _currentLocation = _route.Count;

        for (int i = 0; i < _route.Count; i++)
        {
            if (_economy.IsLocationCompleted(_route[i].Id) == false)
            {
                _currentLocation = i;

                break;
            }
        }
    }

    private List<LocationRow> GetLocations(string systemId)
    {
        List<LocationRow> found = new List<LocationRow>();

        foreach (LocationRow location in _locations.Rows)
        {
            if (location.System == systemId && location.Id != _testLocationId && location.IsBranch == false)
                found.Add(location);
        }

        found.Sort((left, right) => left.Order.CompareTo(right.Order));

        return found;
    }

    private void ForgetView()
    {
        _lastView = null;
    }

    private void ShowGalaxy()
    {
        Open(_galaxyView, GalaxyTitle, false);
        _galaxyOverlay.HideNow();
    }

    private void ShowArm()
    {
        Open(_armView, ArmTitle, true);

        int currentSystem = GetSystemIndex(_currentLocation);

        for (int i = 0; i < _systemNodes.Length; i++)
        {
            bool exists = i < _systemRoute.Count;

            _systemNodes[i].gameObject.SetActive(exists);

            if (exists)
                _systemNodes[i].Show(i, SystemTitle + _systemRoute[i].Order, GetSystemState(i, currentSystem), _starColors[i]);
        }

        ShowRoute(_armDone, _armRest, _systemPositions, Mathf.Min(currentSystem, _systemPositions.Length - 1),
            currentSystem >= _systemPositions.Length);
    }

    private void ShowSystem(int index)
    {
        _openedSystem = index;
        Select(NoSelection);
        Open(_systemView, SystemTitle + _systemRoute[index].Order, true);
        _systemBackground.sprite = _systemSprites[index];

        int first = GetFirstLocation(index);
        List<LocationRow> locations = GetLocations(_systemRoute[index].Id);
        Vector2[] positions = _locationPositions[index].Points;

        for (int i = 0; i < _locationNodes.Length; i++)
        {
            bool exists = i < locations.Count && i < positions.Length;

            _locationNodes[i].gameObject.SetActive(exists);

            if (exists == false)
                continue;

            RectTransform node = (RectTransform)_locationNodes[i].transform;

            node.anchorMin = node.anchorMax = new Vector2(positions[i].x, 1f - positions[i].y);
            node.anchoredPosition = Vector2.zero;
            _locationNodes[i].Show(first + i, GetLabel(first + i), GetLocationState(first + i), _starColors[index]);
            _locationNodes[i].SetStars(_economy.GetStars(_route[first + i].Id));
        }

        int count = Mathf.Min(locations.Count, positions.Length);
        Vector2[] route = new Vector2[count];

        Array.Copy(positions, route, count);
        ShowRoute(_routeDone, _routeRest, route, Mathf.Clamp(_currentLocation - first, -1, count - 1),
            _currentLocation - first >= count);
        ShowBranches(locations, route);
    }

    private void ShowBranches(List<LocationRow> locations, Vector2[] positions)
    {
        _shownBranches.Clear();

        for (int i = 0; i < positions.Length; i++)
        {
            foreach (LocationRow branch in _locations.GetBranches(locations[i].Id))
            {
                if (_shownBranches.Count >= _branchNodes.Length)
                    break;

                ShowBranch(_shownBranches.Count, branch, positions[i]);
                _shownBranches.Add(branch);
            }
        }

        for (int i = _shownBranches.Count; i < _branchNodes.Length; i++)
        {
            _branchNodes[i].gameObject.SetActive(false);
            _branchLinks[i].gameObject.SetActive(false);
        }
    }

    private void ShowBranch(int index, LocationRow branch, Vector2 parent)
    {
        Vector2 position = GetBranchPosition(parent);
        RectTransform node = (RectTransform)_branchNodes[index].transform;

        _branchNodes[index].gameObject.SetActive(true);
        node.anchorMin = node.anchorMax = new Vector2(position.x, 1f - position.y);
        node.anchoredPosition = Vector2.zero;
        _branchNodes[index].Show(index, branch.Title, GetBranchState(branch), _branchColor);
        _branchNodes[index].SetStars(_economy.GetStars(branch.Id));

        UiPolyline link = _branchLinks[index];

        link.gameObject.SetActive(true);
        link.color = new Color(_branchColor.r, _branchColor.g, _branchColor.b, _restRouteColor.a);
        link.SetStyle(RouteWidth * 0.75f, Dash * 0.6f, Gap * 0.6f, 0f);
        link.SetPoints(new List<Vector2> { parent, position });
    }

    private Vector2 GetBranchPosition(Vector2 parent)
    {
        float side = parent.x < _starCentre.x ? -1f : 1f;

        return parent + new Vector2(side * BranchDistance / ImageWidth, 0f);
    }

    private MapNodeState GetBranchState(LocationRow branch)
    {
        if (_economy.IsLocationCompleted(branch.Id))
            return MapNodeState.Completed;

        return _economy.IsLocationCompleted(branch.BranchOf) ? MapNodeState.Current : MapNodeState.Locked;
    }

    private void ShowRoute(UiPolyline done, UiPolyline rest, Vector2[] points, int reached, bool isAllDone)
    {
        List<Vector2> donePoints = new List<Vector2>();
        List<Vector2> restPoints = new List<Vector2>();
        int split = isAllDone ? points.Length - 1 : reached;

        for (int i = 0; i < points.Length; i++)
        {
            if (i <= split)
                donePoints.Add(points[i]);

            if (i >= Mathf.Max(split, 0))
                restPoints.Add(points[i]);
        }

        if (split < 0)
            donePoints.Clear();

        done.color = _doneRouteColor;
        done.SetStyle(RouteWidth, Dash, Gap, DashSpeed);
        done.SetPoints(donePoints);
        rest.color = _restRouteColor;
        rest.SetStyle(RouteWidth * 0.75f, Dash, Gap, 0f);
        rest.SetPoints(isAllDone ? new List<Vector2>() : restPoints);
    }

    private void Open(GameObject view, string title, bool canGoBack)
    {
        if (view != _systemView)
            Select(NoSelection);

        _lastView = view;
        _galaxyView.SetActive(view == _galaxyView);
        _armView.SetActive(view == _armView);
        _systemView.SetActive(view == _systemView);
        _title.text = title;
        _info.text = string.Empty;
        _backButton.gameObject.SetActive(canGoBack);
    }

    private MapNodeState GetSystemState(int system, int currentSystem)
    {
        if (system < currentSystem)
            return MapNodeState.Completed;

        return system == currentSystem ? MapNodeState.Current : MapNodeState.Locked;
    }

    private MapNodeState GetLocationState(int index)
    {
        if (index < _currentLocation)
            return MapNodeState.Completed;

        return index == _currentLocation ? MapNodeState.Current : MapNodeState.Locked;
    }

    private int GetSystemIndex(int locationIndex)
    {
        if (locationIndex >= _route.Count)
            return _systemRoute.Count;

        string systemId = _route[locationIndex].System;

        return _systemRoute.FindIndex(system => system.Id == systemId);
    }

    private int GetCurrentSystem()
    {
        return Mathf.Min(GetSystemIndex(_currentLocation), _systemRoute.Count - 1);
    }

    private int GetFirstLocation(int system)
    {
        int first = 0;

        for (int i = 0; i < system; i++)
            first += GetLocations(_systemRoute[i].Id).Count;

        return first;
    }

    private string GetLabel(int index)
    {
        LocationRow location = _route[index];
        int system = _systemRoute.FindIndex(row => row.Id == location.System);

        return _systemRoute[system].Order + "-" + location.Order;
    }

    private void Back()
    {
        if (_systemView.activeSelf)
            ShowArm();
        else
            ShowGalaxy();
    }

    private void OnRegionClicked(MapRegion region)
    {
        if (region.IsAvailable)
            ShowArm();
        else
            _toast.Show(DemoOnly);
    }

    private void OnRegionHovered(MapRegion region, bool isHovered)
    {
        _galaxyOverlay.Show(isHovered);
    }

    private void OnSystemClicked(MapNode node)
    {
        ShowSystem(node.Index);
    }

    private void OnLocationClicked(MapNode node)
    {
        LocationRow location = _route[node.Index];
        bool canSelect = node.State != MapNodeState.Locked && _waves.HasLocation(location.Id);
        string state = node.State == MapNodeState.Completed ? "COMPLETED"
            : node.State == MapNodeState.Current ? "NEXT" : "LOCKED";

        _info.text = "LOCATION " + GetLabel(node.Index) + "     " + location.Levels + " LEVELS     " + state;
        Select(canSelect ? location.Id : NoSelection);
    }

    private void OnBranchClicked(MapNode node)
    {
        LocationRow branch = _shownBranches[node.Index];
        LocationRow parent = _locations.GetRow(branch.BranchOf);
        bool canSelect = node.State != MapNodeState.Locked && _waves.HasLocation(branch.Id);
        string state = node.State == MapNodeState.Completed ? "COMPLETED"
            : node.State == MapNodeState.Current ? "OPEN" : "LOCKED UNTIL " + GetLabel(_route.IndexOf(parent)) + " IS CLEARED";
        string levels = branch.Levels == 1 ? " LEVEL     " : " LEVELS     ";

        _info.text = branch.Title + "     " + branch.Levels + levels + state;
        Select(canSelect ? branch.Id : NoSelection);
    }

    private void Select(string locationId)
    {
        _selected = locationId;

        foreach (MapNode node in _locationNodes)
            node.SetSelected(node.gameObject.activeSelf && locationId != NoSelection && _route[node.Index].Id == locationId);

        for (int i = 0; i < _branchNodes.Length; i++)
            _branchNodes[i].SetSelected(i < _shownBranches.Count && _branchNodes[i].gameObject.activeSelf
                && _shownBranches[i].Id == locationId);

        bool isReplay = locationId != NoSelection && _economy.IsLocationCompleted(locationId);

        _playLabel.text = isReplay ? ReplayText : PlayText;
        _playButton.gameObject.SetActive(_canPlay || locationId != NoSelection);
    }

    private void OnMenuClicked()
    {
        MenuRequested?.Invoke();
    }

    private void OnDockClicked()
    {
        DockRequested?.Invoke();
    }

    private void OnPlayClicked()
    {
        if (_selected == NoSelection)
            PlayRequested?.Invoke();
        else
            LocationPlayRequested?.Invoke(_selected);
    }

#if UNITY_EDITOR
    public void Configure(Vector2[] systemPositions, Sprite[] systemSprites, Color[] starColors,
        LocationPositions[] locationPositions)
    {
        _systemPositions = systemPositions;
        _systemSprites = systemSprites;
        _starColors = starColors;
        _locationPositions = locationPositions;
    }
#endif
}

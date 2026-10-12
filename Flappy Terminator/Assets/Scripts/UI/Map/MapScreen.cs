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
    private const string ToArmText = "ARM";
    private const string ToGalaxyText = "GALAXY";
    private const string GateText = "STAR GATE";
    private const string ToSystemText = "SYSTEM";
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
    [SerializeField] private MapNode[] _armBranchNodes;
    [SerializeField] private BranchPlacement[] _branchPlacements;
    [SerializeField] private StarGateView _starGate;
    [SerializeField] private UiPolyline _gateLink;
    [SerializeField] private Vector2[] _gatePositions;
    [SerializeField] private StarGateView _voidGate;
    [SerializeField] private UiPolyline _voidLink;
    [SerializeField] private VoidPlacement[] _voidPlacements;
    [SerializeField] private Sprite _voidSprite;
    [SerializeField] private Vector2 _starCentre = new Vector2(0.5f, 0.4907f);
    [SerializeField] private Color _branchColor = new Color(0.7f, 0.62f, 1f);
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _info;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _backLabel;
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
    private readonly List<VoidPlacement> _shownArmVoids = new List<VoidPlacement>();
    private int _currentLocation;
    private int _openedSystem = -1;
    private string _selected = NoSelection;
    private GameObject _lastView;
    private string _openedVoid;
    private VoidPlacement _shownVoid;
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

        foreach (MapNode node in _armBranchNodes)
            node.Clicked += OnBranchClicked;

        _starGate.Clicked += OnGateClicked;
        _voidGate.Clicked += OnVoidGateClicked;
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

        foreach (MapNode node in _armBranchNodes)
            node.Clicked -= OnBranchClicked;

        _starGate.Clicked -= OnGateClicked;
        _voidGate.Clicked -= OnVoidGateClicked;
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
        else if (_lastView == _systemView && _openedVoid != null)
            ShowVoid(_openedVoid);
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
            if (location.System == systemId && location.IsBranch == false)
                found.Add(location);
        }

        found.Sort((left, right) => left.Order.CompareTo(right.Order));

        return found;
    }

    private void ForgetView()
    {
        _lastView = null;
        _openedVoid = null;
    }

    private void ShowGalaxy()
    {
        _openedVoid = null;
        Open(_galaxyView, GalaxyTitle, false);
        _galaxyOverlay.HideNow();
    }

    private void ShowArm()
    {
        _openedVoid = null;
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
        ShowArmVoids();
    }

    private void ShowArmVoids()
    {
        _shownArmVoids.Clear();

        foreach (VoidPlacement placement in _voidPlacements)
        {
            if (placement.Arm == Vector2.zero || _locations.IsVoid(placement.Id) == false
                || _shownArmVoids.Count >= _armBranchNodes.Length)
                continue;

            int index = _shownArmVoids.Count;
            MapNode node = _armBranchNodes[index];
            RectTransform rect = (RectTransform)node.transform;

            node.gameObject.SetActive(true);
            rect.anchorMin = rect.anchorMax = new Vector2(placement.Arm.x, 1f - placement.Arm.y);
            rect.anchoredPosition = Vector2.zero;
            node.Show(index, placement.Title, IsVoidOpen(placement.Id) ? MapNodeState.Open : MapNodeState.Locked,
                _branchColor);
            _shownArmVoids.Add(placement);
        }

        for (int i = _shownArmVoids.Count; i < _armBranchNodes.Length; i++)
            _armBranchNodes[i].gameObject.SetActive(false);
    }

    private bool IsVoidOpen(string voidId)
    {
        List<LocationRow> locations = _locations.GetVoid(voidId);

        return _economy.IsTestSession || (locations.Count > 0 && _economy.IsLocationCompleted(locations[0].BranchOf));
    }

    private void ShowVoid(string voidId)
    {
        VoidPlacement placement = Array.Find(_voidPlacements, item => item.Id == voidId);
        List<LocationRow> locations = _locations.GetVoid(voidId);

        Select(NoSelection);
        Open(_systemView, UiText.Title(voidId), true);
        _openedVoid = voidId;
        _backLabel.text = ToSystemText;
        _systemBackground.sprite = _voidSprite;

        foreach (MapNode node in _locationNodes)
            node.gameObject.SetActive(false);

        _routeDone.SetPoints(new List<Vector2>());
        _routeRest.SetPoints(new List<Vector2>());
        _starGate.gameObject.SetActive(false);
        _gateLink.gameObject.SetActive(false);
        _voidGate.gameObject.SetActive(false);
        _voidLink.gameObject.SetActive(false);
        _shownBranches.Clear();

        for (int i = 0; i < _branchNodes.Length; i++)
        {
            bool exists = placement != null && i < locations.Count && i < placement.Locations.Length;

            _branchNodes[i].gameObject.SetActive(exists);
            _branchLinks[i].gameObject.SetActive(false);

            if (exists == false)
                continue;

            RectTransform rect = (RectTransform)_branchNodes[i].transform;
            Vector2 position = placement.Locations[i];

            rect.anchorMin = rect.anchorMax = new Vector2(position.x, 1f - position.y);
            rect.anchoredPosition = Vector2.zero;
            _branchNodes[i].Show(i, locations[i].Title, GetBranchState(locations[i]), _branchColor);
            _branchNodes[i].SetStars(_economy.GetStars(locations[i].Id));
            _shownBranches.Add(locations[i]);
        }
    }

    private void ShowVoidGate(List<LocationRow> locations, Vector2[] route)
    {
        _shownVoid = null;

        foreach (VoidPlacement placement in _voidPlacements)
        {
            List<LocationRow> inVoid = _locations.GetVoid(placement.Id);
            int parent = inVoid.Count == 0 ? -1 : locations.FindIndex(location => location.Id == inVoid[0].BranchOf);

            if (parent < 0 || parent >= route.Length || placement.System == Vector2.zero)
                continue;

            bool isOpen = IsVoidOpen(placement.Id);
            RectTransform rect = (RectTransform)_voidGate.transform;

            _shownVoid = placement;
            rect.anchorMin = rect.anchorMax = new Vector2(placement.System.x, 1f - placement.System.y);
            rect.anchoredPosition = Vector2.zero;
            _voidGate.Show(placement.Title, isOpen);
            _voidLink.color = new Color(_branchColor.r, _branchColor.g, _branchColor.b, isOpen ? 1f : _restRouteColor.a);
            _voidLink.SetStyle(RouteWidth * 0.75f, Dash * 0.6f, Gap * 0.6f, isOpen ? DashSpeed : 0f);
            _voidLink.SetPoints(new List<Vector2> { route[parent], placement.System });

            break;
        }

        _voidGate.gameObject.SetActive(_shownVoid != null);
        _voidLink.gameObject.SetActive(_shownVoid != null);
    }

    private void ShowSystem(int index)
    {
        _openedSystem = index;
        _openedVoid = null;
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
        ShowGate(index, locations, route);
        ShowVoidGate(locations, route);
    }

    private void ShowGate(int system, List<LocationRow> locations, Vector2[] route)
    {
        bool exists = system + 1 < _systemRoute.Count && system < _gatePositions.Length
            && _gatePositions[system] != Vector2.zero && route.Length > 0;

        _starGate.gameObject.SetActive(exists);
        _gateLink.gameObject.SetActive(exists);

        if (exists == false)
            return;

        Vector2 position = _gatePositions[system];
        RectTransform rect = (RectTransform)_starGate.transform;
        bool isOpen = _economy.IsTestSession || _economy.IsLocationCompleted(locations[route.Length - 1].Id);

        rect.anchorMin = rect.anchorMax = new Vector2(position.x, 1f - position.y);
        rect.anchoredPosition = Vector2.zero;
        _starGate.Show(GateText, isOpen);
        _gateLink.color = isOpen ? _doneRouteColor : _restRouteColor;
        _gateLink.SetStyle(RouteWidth * 0.75f, Dash, Gap, isOpen ? DashSpeed : 0f);
        _gateLink.SetPoints(new List<Vector2> { route[route.Length - 1], position });
    }

    private void ShowBranches(List<LocationRow> locations, Vector2[] positions)
    {
        _shownBranches.Clear();

        for (int i = 0; i < positions.Length; i++)
        {
            foreach (LocationRow branch in _locations.GetBranches(locations[i].Id))
            {
                if (branch.IsInVoid)
                    continue;

                if (_shownBranches.Count >= _branchNodes.Length)
                    break;

                ShowBranch(_shownBranches.Count, branch, positions[i], GetBranchPosition(branch, positions[i]));
                _shownBranches.Add(branch);
            }
        }

        for (int i = _shownBranches.Count; i < _branchNodes.Length; i++)
        {
            _branchNodes[i].gameObject.SetActive(false);
            _branchLinks[i].gameObject.SetActive(false);
        }
    }

    private void ShowBranch(int index, LocationRow branch, Vector2 parent, Vector2 position)
    {
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

    private Vector2 GetBranchPosition(LocationRow branch, Vector2 parent)
    {
        foreach (BranchPlacement placement in _branchPlacements)
        {
            if (placement.Id == branch.Id && placement.System != Vector2.zero)
                return placement.System;
        }

        float side = parent.x < _starCentre.x ? -1f : 1f;

        return parent + new Vector2(side * BranchDistance / ImageWidth, 0f);
    }

    private MapNodeState GetBranchState(LocationRow branch)
    {
        if (_economy.IsLocationCompleted(branch.Id))
            return MapNodeState.Completed;

        if (_economy.IsTestSession)
            return MapNodeState.Open;

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
        _backLabel.text = view == _systemView ? ToArmText : ToGalaxyText;
    }

    private MapNodeState GetSystemState(int system, int currentSystem)
    {
        if (system < currentSystem)
            return MapNodeState.Completed;

        if (_economy.IsTestSession && system != currentSystem)
            return MapNodeState.Open;

        return system == currentSystem ? MapNodeState.Current : MapNodeState.Locked;
    }

    private MapNodeState GetLocationState(int index)
    {
        if (_economy.IsTestSession)
            return _economy.IsLocationCompleted(_route[index].Id) ? MapNodeState.Completed : MapNodeState.Open;

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
        if (_openedVoid != null)
            ShowSystem(_openedSystem);
        else if (_systemView.activeSelf)
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
            : node.State == MapNodeState.Current ? "NEXT"
            : node.State == MapNodeState.Open ? GetOpenCaption(location) : "LOCKED";

        _info.text = "LOCATION " + GetLabel(node.Index) + "     " + location.Levels + GetLevelsWord(location) + state;
        Select(canSelect ? location.Id : NoSelection);
    }

    private void OnBranchClicked(MapNode node)
    {
        if (Array.IndexOf(_armBranchNodes, node) >= 0)
        {
            OnVoidEntered(_shownArmVoids[node.Index]);

            return;
        }

        LocationRow branch = _shownBranches[node.Index];
        LocationRow parent = _locations.GetRow(branch.BranchOf);
        bool canSelect = node.State != MapNodeState.Locked && _waves.HasLocation(branch.Id);
        string state = node.State == MapNodeState.Completed ? "COMPLETED"
            : node.State == MapNodeState.Current || node.State == MapNodeState.Open ? GetOpenCaption(branch)
            : "LOCKED UNTIL " + GetLabel(_route.IndexOf(parent)) + " IS CLEARED";
        _info.text = branch.Title + "     " + branch.Levels + GetLevelsWord(branch) + state;
        Select(canSelect ? branch.Id : NoSelection);
    }

    private string GetOpenCaption(LocationRow location)
    {
        return _waves.HasLocation(location.Id) ? "OPEN" : "NO WAVES YET";
    }

    private string GetLevelsWord(LocationRow location)
    {
        return location.Levels == 1 ? " LEVEL     " : " LEVELS     ";
    }

    private void OnGateClicked()
    {
        int next = _openedSystem + 1;

        if (_starGate.IsOpen)
        {
            ShowSystem(next);

            return;
        }

        int last = GetFirstLocation(_openedSystem) + GetLocations(_systemRoute[_openedSystem].Id).Count - 1;

        _info.text = GateText + " TO " + SystemTitle + _systemRoute[next].Order + "     LOCKED UNTIL " + GetLabel(last)
            + " IS CLEARED";
        Select(NoSelection);
    }

    private void OnVoidGateClicked()
    {
        if (_shownVoid != null)
            OnVoidEntered(_shownVoid);
    }

    private void OnVoidEntered(VoidPlacement placement)
    {
        List<LocationRow> locations = _locations.GetVoid(placement.Id);

        if (IsVoidOpen(placement.Id) == false)
        {
            LocationRow parent = _locations.GetRow(locations[0].BranchOf);

            _info.text = placement.Title + "     LOCKED UNTIL " + GetLabel(_route.IndexOf(parent)) + " IS CLEARED";
            Select(NoSelection);

            return;
        }

        _openedSystem = _systemRoute.FindIndex(system => system.Id == locations[0].System);
        ShowVoid(placement.Id);
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
        LocationPositions[] locationPositions, BranchPlacement[] branchPlacements, Vector2[] gatePositions,
        VoidPlacement[] voidPlacements, Sprite voidSprite)
    {
        _voidPlacements = voidPlacements;
        _voidSprite = voidSprite;
        _branchPlacements = branchPlacements;
        _gatePositions = gatePositions;
        _systemPositions = systemPositions;
        _systemSprites = systemSprites;
        _starColors = starColors;
        _locationPositions = locationPositions;
    }
#endif
}

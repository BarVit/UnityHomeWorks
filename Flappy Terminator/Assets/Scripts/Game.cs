using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private InputReader _input;
    [SerializeField] private CheatCodeReader _cheatCode;
    [SerializeField] private CheatCodeReader _systemCheatCode;
    [SerializeField] private SystemSkip _systemSkip;
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private Hangar _hangar;
    [SerializeField] private ModuleRack _moduleRack;
    [SerializeField] private SkillBook _skills;
    [SerializeField] private BattleModules _battleModules;
    [SerializeField] private BattleStats _stats;
    [SerializeField] private WarpJump _warpJump;
    [SerializeField] private CutsceneSkip _cutsceneSkip;
    [SerializeField] private ScreenNavigator _screens;
    [SerializeField] private PauseController _pause;
    [SerializeField] private LocationRunner _runner;
    [SerializeField] private float _skippedDeathSoundFade = 0.5f;

    private readonly BattleSoundFade _soundFade = new BattleSoundFade();

    private Coroutine _gameOverCountdown;
    private Coroutine _soundFading;
    private bool _isLevelOver;
    private bool _isRunning;
    private bool _isDeathSkipped;
    private DockReason _dockReason;
    private Reward _dockEarned;
    private string _queuedLocationId;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
        _playerShip.Died += LoseLevel;
        _warpJump.Finished += FinishWarp;
        _screens.StartRequested += StartGame;
        _screens.SessionResumeRequested += ResumeSession;
        _screens.StartTestRequested += StartTest;
        _screens.TestResetRequested += StartTest;
        _screens.ResumeRequested += Resume;
        _screens.RestartFromPauseRequested += RestartFromPause;
        _screens.PlayRequested += PlayFromDock;
        _screens.LocationPlayRequested += PlayLocation;
        _screens.ResultClosed += ShowClearedDock;
        _input.PauseToggled += TogglePause;
        _input.Shot += SkipCutscene;
        _input.Tapped += SkipCutscene;
        _cheatCode.Entered += WinLevelInstantly;
        _systemCheatCode.Entered += CompleteSystemInstantly;
    }

    private void Start()
    {
        _pause.Resume();
        _playerShip.DisableControl();
        _playerShip.ReturnToStart();
        _screens.ShowStart();
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
        _playerShip.Died -= LoseLevel;
        _warpJump.Finished -= FinishWarp;
        _screens.StartRequested -= StartGame;
        _screens.SessionResumeRequested -= ResumeSession;
        _screens.StartTestRequested -= StartTest;
        _screens.TestResetRequested -= StartTest;
        _screens.ResumeRequested -= Resume;
        _screens.RestartFromPauseRequested -= RestartFromPause;
        _screens.PlayRequested -= PlayFromDock;
        _screens.LocationPlayRequested -= PlayLocation;
        _screens.ResultClosed -= ShowClearedDock;
        _input.PauseToggled -= TogglePause;
        _input.Shot -= SkipCutscene;
        _input.Tapped -= SkipCutscene;
        _cheatCode.Entered -= WinLevelInstantly;
        _systemCheatCode.Entered -= CompleteSystemInstantly;
    }

    private void SkipCutscene()
    {
        if (_pause.IsPaused)
            return;

        if (_gameOverCountdown != null)
        {
            SkipDeath();

            return;
        }

        if (_warpJump.CanSkip == false)
            return;

        _input.ConsumeFire();
        _warpJump.Skip();
    }

    private void SkipDeath()
    {
        if (_cutsceneSkip.CanSkip == false)
            return;

        _input.ConsumeFire();
        _isDeathSkipped = true;
    }

    private void TogglePause()
    {
        if (_screens.TryGoBack())
            return;

        if (_isRunning == false)
            return;

        if (_pause.IsPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        _pause.Pause();
        _screens.ShowPause();
    }

    private void Resume()
    {
        _pause.Resume();
        _screens.HidePause();
    }

    private void StartGame()
    {
        _queuedLocationId = null;
        _runner.StartGame();
        _hangar.StartNewSession();
        EquipShip();

        StartLevel();
    }

    private void StartTest()
    {
        _queuedLocationId = null;
        _hangar.StartTestSession(_playerShip.Shooter.TestWeaponId);
        _dockReason = DockReason.TestStart;
        _screens.CanResumeSession = false;
        _screens.ShowDock(default, false, DockReason.TestStart);
    }

    private void PlayLocation(string locationId)
    {
        _queuedLocationId = null;
        _runner.PlayLocation(locationId);
        EquipShip();

        StartLevel();
    }

    private void PlayFromDock()
    {
        if (_dockReason == DockReason.TestStart)
            _runner.StartGame();
        else if (_dockReason == DockReason.Destroyed)
            _runner.RetryLocation();
        else if (_queuedLocationId != null)
            _runner.PlayLocation(_queuedLocationId);
        else
            _runner.StartNextLocation();

        _queuedLocationId = null;
        EquipShip();

        StartLevel();
    }

    private void RestartRun()
    {
        _runner.RestartRun();

        StartLevel();
    }

    private void RestartFromPause()
    {
        Resume();
        RestartRun();
    }

    private void FinishWarp()
    {
        bool wasLastLevel = _runner.IsLastLevel;

        _stats.CompleteLevel(_runner.PendingReward);
        _runner.BankLevel();

        if (wasLastLevel)
            CompleteLocation();
        else
            StartNextLevel();
    }

    private void StartNextLevel()
    {
        _runner.Advance();

        StartLevel();
    }

    private void CompleteLocation()
    {
        _runner.CompleteLocation(_stats.Stars);
        _isRunning = false;
        _warpJump.Stop();
        _asteroidSpawner.StopSpawn();
        _playerShip.DisableControl();
        _playerShip.ReturnToStart();

        ClearBattlefield();
        _stats.PayBonus();
        _screens.ShowResult(_stats, _runner.BankedReward);
    }

    private void ShowClearedDock()
    {
        ShowDock(DockReason.Cleared);
    }

    private void StartLevel()
    {
        StopGameOverCountdown();
        StopSoundFade();
        _warpJump.Stop();

        _isLevelOver = false;
        _isRunning = true;
        _screens.HideOverlays();
        _battleModules.ResetForLevel();
        _playerShip.Restart();
        TrackLevel();
        _enemySpawner.StartLevel(_runner.LocationId, _runner.Level);
        _asteroidSpawner.StartSpawn();
    }

    private void TrackLevel()
    {
        if (_runner.Level == 1)
            _stats.BeginLocation(_enemySpawner.CountPlanned(_runner.LocationId, _runner.LevelCount));

        _stats.BeginLevel();
    }

    private void LoseLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;

        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _gameOverCountdown = StartCoroutine(ShowDeathScreen());
    }

    private void WinLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.FinishWave();
        _asteroidSpawner.StopSpawn();
        _warpJump.Play();
    }

    private void WinLevelInstantly()
    {
        if (_isRunning == false || _isLevelOver || _pause.IsPaused)
            return;

        _stats.ForgiveLevel();
        _playerShip.Shooter.ReleaseAll();
        _enemySpawner.DefeatAll();
    }

    private void CompleteSystemInstantly()
    {
        if (_isRunning == false || _isLevelOver || _pause.IsPaused)
            return;

        Reward earned = _systemSkip.Complete(_runner.LocationId, _runner.BankedReward, out string nextLocationId);

        _runner.DropPendingReward();
        _isRunning = false;
        _asteroidSpawner.StopSpawn();
        ClearBattlefield();
        _playerShip.ReturnToStart();

        bool canPlay = nextLocationId != null;

        _queuedLocationId = nextLocationId;
        _dockReason = DockReason.Cleared;
        _dockEarned = earned;
        _screens.CanResumeSession = canPlay;
        _screens.ShowDock(earned, canPlay, DockReason.Cleared);
    }

    private IEnumerator ShowDeathScreen()
    {
        float end = Time.time + _playerShip.DeathDuration;

        _isDeathSkipped = false;
        _cutsceneSkip.Begin();

        while (Time.time < end && _isDeathSkipped == false)
            yield return null;

        _cutsceneSkip.End();
        _gameOverCountdown = null;
        _isRunning = false;

        if (_isDeathSkipped)
            FadeBattleSounds();

        _runner.BankLevel();
        ShowDock(DockReason.Destroyed);
    }

    private void ShowDock(DockReason reason)
    {
        ClearBattlefield();

        bool canPlay = reason != DockReason.Cleared || _runner.HasNextLocation;

        _dockReason = reason;
        _dockEarned = reason == DockReason.Cleared ? _runner.BankedReward + _stats.Bonus : _runner.BankedReward;
        _screens.CanResumeSession = canPlay;
        _screens.ShowDock(_dockEarned, canPlay, reason);
    }

    private void ClearBattlefield()
    {
        _warpJump.Stop();
        _enemySpawner.ClearField();
        _asteroidSpawner.Clear();
        _playerShip.DisableControl();
        _playerShip.Shooter.ReleaseAll();
        _battleModules.ResetForLevel();
    }

    private void ResumeSession()
    {
        _screens.ShowDock(_dockEarned, true, _dockReason);
    }

    private void EquipShip()
    {
        string weaponId = _hangar.WeaponId;

        _hangar.Equip(_playerShip.Shooter, _skills.GetRateScale(weaponId), _skills.GetReloadShift(weaponId));
        _moduleRack.Equip(_battleModules, _skills.GetShieldScale());
        _skills.Equip(_playerShip);
    }

    private void FadeBattleSounds()
    {
        StopSoundFade();
        _soundFading = StartCoroutine(_soundFade.Fade(_skippedDeathSoundFade));
    }

    private void StopSoundFade()
    {
        if (_soundFading == null)
            return;

        StopCoroutine(_soundFading);
        _soundFading = null;
        _soundFade.Finish();
    }

    private void StopGameOverCountdown()
    {
        if (_gameOverCountdown == null)
            return;

        StopCoroutine(_gameOverCountdown);
        _gameOverCountdown = null;
        _cutsceneSkip.End();
    }
}

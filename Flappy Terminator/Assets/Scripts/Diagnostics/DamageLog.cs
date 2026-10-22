using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class DamageLog
{
    public const string Napalm = "napalm";
    public const string Sentry = "sentry";
    public const string Asteroid = "asteroid";
    public const string Boundary = "boundary";

    private const int MaxLines = 100000;
    private const string Header = "time\tlocation\tlevel\tevent\tsource\ttarget\ttarget_ref\tdamage\tabsorbed\tapplied"
        + "\thp_before\thp_after\thp_max\tnote";
    private const string Separator = "\t";
    private const string Unknown = "?";

    private static readonly List<string> Lines = new List<string>();

    private static string _location = string.Empty;
    private static int _level;
    private static string _source;

    public static void BeginLevel(string location, int level)
    {
        _location = location;
        _level = level;
        Write("level_start", string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, 0, 0, string.Empty);
    }

    public static void SetSource(string source)
    {
        _source = source;
    }

    public static string TakeSource()
    {
        string source = string.IsNullOrEmpty(_source) ? Unknown : _source;

        _source = null;

        return source;
    }

    public static void Hit(string eventName, string source, string target, int targetRef, int damage, int absorbed,
        int applied, int hpBefore, int hpAfter, int hpMax, string note = "")
    {
        Write(eventName, source, target, targetRef.ToString(CultureInfo.InvariantCulture), damage, absorbed, applied,
            hpBefore, hpAfter, hpMax, note);
    }

    public static void Burn(string eventName, string target, int targetRef, float amount, string note = "")
    {
        Write(eventName, Napalm, target, targetRef.ToString(CultureInfo.InvariantCulture), 0, 0, 0, 0, 0, 0,
            amount.ToString("0.###", CultureInfo.InvariantCulture) + (note.Length > 0 ? " " + note : string.Empty));
    }

    public static void Save()
    {
        string name = "damage-log-v" + Application.version + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".tsv";
        string text = GetText();

#if UNITY_WEBGL && !UNITY_EDITOR
        DamageLogDownload(name, text);
#else
        string path = Path.Combine(Application.persistentDataPath, name);
        File.WriteAllText(path, text, Encoding.UTF8);
        Debug.Log("Лог урона: " + Lines.Count + " строк -> " + path);
#endif
    }

    public static string GetText()
    {
        StringBuilder text = new StringBuilder(Header).Append('\n');

        foreach (string line in Lines)
            text.Append(line).Append('\n');

        return text.ToString();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DamageLogDownload(string name, string text);
#endif

    private static void Write(string eventName, string source, string target, string targetRef, int damage,
        int absorbed, int applied, int hpBefore, int hpAfter, int hpMax, string note)
    {
        if (Lines.Count >= MaxLines)
            Lines.RemoveRange(0, MaxLines / 2);

        Lines.Add(string.Join(Separator, Time.time.ToString("0.000", CultureInfo.InvariantCulture), _location,
            _level.ToString(CultureInfo.InvariantCulture), eventName, source, target, targetRef,
            damage.ToString(CultureInfo.InvariantCulture), absorbed.ToString(CultureInfo.InvariantCulture),
            applied.ToString(CultureInfo.InvariantCulture), hpBefore.ToString(CultureInfo.InvariantCulture),
            hpAfter.ToString(CultureInfo.InvariantCulture), hpMax.ToString(CultureInfo.InvariantCulture), note));
    }
}

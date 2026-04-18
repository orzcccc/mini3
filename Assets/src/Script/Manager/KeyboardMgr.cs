using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 调试键盘输入管理器，统一处理运行时快捷键注册与触发。
/// </summary>
public sealed class KeyboardMgr : Singleton<KeyboardMgr>
{
    private readonly Dictionary<KeyCode, Action> m_KeyDownHandlers = new Dictionary<KeyCode, Action>();

    protected override void OnInit()
    {
        base.OnInit();
        RegisterDefaultDebugShortcuts();
    }

    public void RegisterKeyDown(KeyCode keyCode, Action handler)
    {
        if (handler == null)
        {
            return;
        }

        if (m_KeyDownHandlers.TryGetValue(keyCode, out Action existingHandler))
        {
            m_KeyDownHandlers[keyCode] = existingHandler + handler;
            return;
        }

        m_KeyDownHandlers[keyCode] = handler;
    }

    public void UnregisterKeyDown(KeyCode keyCode, Action handler)
    {
        if (handler == null)
        {
            return;
        }

        if (!m_KeyDownHandlers.TryGetValue(keyCode, out Action existingHandler))
        {
            return;
        }

        existingHandler -= handler;
        if (existingHandler == null)
        {
            m_KeyDownHandlers.Remove(keyCode);
            return;
        }

        m_KeyDownHandlers[keyCode] = existingHandler;
    }

    public void ClearAll()
    {
        m_KeyDownHandlers.Clear();
    }

    public void Update()
    {
        foreach (KeyValuePair<KeyCode, Action> pair in m_KeyDownHandlers)
        {
            if (!Input.GetKeyDown(pair.Key))
            {
                continue;
            }

            pair.Value?.Invoke();
        }
    }

    private void RegisterDefaultDebugShortcuts()
    {
        RegisterKeyDown(KeyCode.F2, OpenGMView);
    }

    private static void OpenGMView()
    {
        UIMgr.inst.Open("GMView");
    }
}

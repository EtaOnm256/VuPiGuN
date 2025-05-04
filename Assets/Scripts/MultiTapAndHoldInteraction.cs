using UnityEngine;
using UnityEngine.InputSystem;

internal class MultiTapAndHoldInteraction : IInputInteraction<float>
{
    // 最大のタップ時間[s]
    public float tapTime;

    // 次のタップまでの最大待機時間[s]
    public float tapDelay;

    // 必要なタップ数
    public int tapCount = 2;

    // 入力判定の閾値(0でデフォルト値)
    public float pressPoint;

    // リリース判定の閾値(0でデフォルト値)
    public float releasePoint;

    // マルチタップ＆ホールド後、入力がなくなってから終了するまでの時間
    public float endDelay;

    // タップ状態の内部フェーズ
    private enum TapPhase
    {
        None,
        WaitingForNextRelease,
        WaitingForNextPress,
        WaitingForRelease,
        WaitingForEnd,
    }

    // 設定値かデフォルト値の値を格納するフィールド
    private float tapTimeOrDefault => tapTime > 0.0 ? tapTime : InputSystem.settings.defaultTapTime;
    private float tapDelayOrDefault => tapDelay > 0.0 ? tapDelay : InputSystem.settings.multiTapDelayTime;
    private float pressPointOrDefault => pressPoint > 0 ? pressPoint : InputSystem.settings.defaultButtonPressPoint;
    private float releasePointOrDefault => pressPointOrDefault * InputSystem.settings.buttonReleaseThreshold;

    // Interactionの内部状態
    private TapPhase _currentTapPhase = TapPhase.None;
    private double _currentTapStartTime;
    private double _lastTapReleaseTime;
    private int _currentTapCount;

    private double _lastTapReleaseTime_contious;

    // 最後の入力がマルチタップの末尾かどうか
    private bool lastTapAccepted = false;
    
    /// <summary>
    /// 初期化
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    public static void Initialize()
    {
        // 初回にInteractionを登録する必要がある
        InputSystem.RegisterInteraction<MultiTapAndHoldInteraction>();
    }

    /// <summary>
    /// Interactionの内部処理
    /// </summary>
    public void Process(ref InputInteractionContext context)
    {
        // タイムアウト判定
        /*if (context.timerHasExpired)
        {
            // 最大許容時間を超えてタイムアウトになった場合はキャンセル
            context.Canceled();
            Debug.Log("timeout");
            return;
        }*/

        switch (_currentTapPhase)
        {
            case TapPhase.None:
                // 初期状態

                // タップされたかチェック
                if (context.ControlIsActuated(pressPointOrDefault))
                {
                    _currentTapStartTime = context.time;

                    // 最後の入力の継続かどうか判定
                    if (lastTapAccepted)
                    {
                        if (Time.time - _lastTapReleaseTime_contious > tapDelayOrDefault)
                        { }
                        else
                        {
                            _currentTapCount = tapCount;
                        }
                    }

                    if (++_currentTapCount >= tapCount)
                    {
                        // 必要なタップ数に達したらPerformedコールバック実行
                        _currentTapPhase = TapPhase.WaitingForRelease;
                        context.Started();
                        lastTapAccepted = true;
                        //context.PerformedAndStayPerformed();
                    }
                    else
                    {
                        // 入力がなくなるまで待機
                        _currentTapPhase = TapPhase.WaitingForNextRelease;
                        //context.Started();
                        context.SetTimeout(tapTimeOrDefault);
                    }
                }
                break;

            case TapPhase.WaitingForNextRelease:
                // 入力がなくなるまで待機している状態
                if (!context.ControlIsActuated(releasePointOrDefault))
                {
                    if (context.time - _currentTapStartTime > tapTimeOrDefault)
                    {
                        // 最大許容時間を超えたのでキャンセル
                        context.Canceled();
                        lastTapAccepted = false;
                        break;
                    }

                    // 次の入力待ち状態に遷移
                    _lastTapReleaseTime = context.time;
                    _currentTapPhase = TapPhase.WaitingForNextPress;
                    context.SetTimeout(tapDelayOrDefault);
                }

                break;

            case TapPhase.WaitingForNextPress:
                // 次の入力待ちの状態
                if (context.ControlIsActuated(pressPointOrDefault))
                {
                    if (context.time - _lastTapReleaseTime > tapDelayOrDefault)
                    {
                        // 最大許容時間を超えたのでキャンセル
                        context.Canceled();
                        lastTapAccepted = false;
                        break;
                    }

                    ++_currentTapCount;
                    _currentTapStartTime = context.time;

                    if (_currentTapCount >= tapCount)
                    {
                        // 必要なタップ数に達したので、Performedコールバック通知
                        // 終了まで待機する状態に遷移
                        _currentTapPhase = TapPhase.WaitingForRelease;
                        context.Started();
                        lastTapAccepted = true;
                        //context.PerformedAndStayPerformed();
                    }
                    else
                    {
                        // 必要タップ数に達していないので、入力がなくなるまで待機
                        _currentTapPhase = TapPhase.WaitingForNextRelease;
                        context.SetTimeout(tapTimeOrDefault);
                    }

                    _currentTapStartTime = context.time;
                }

                break;

            case TapPhase.WaitingForRelease:
                // マルチタップ判定後、入力がなくなるまで待機している状態

                // 入力チェック
                if (!context.ControlIsActuated(releasePointOrDefault))
                {
                    // 入力がなくなったので終了
                    _currentTapPhase = TapPhase.WaitingForEnd;
                    _lastTapReleaseTime = context.time;
                    context.SetTimeout(endDelay);
                }

                break;

            case TapPhase.WaitingForEnd:
                // 入力がなくなってからInteractionを終了するまで待機している状態
                if (context.time - _lastTapReleaseTime >= endDelay)
                {
                    _lastTapReleaseTime_contious = Time.time;

                    // 一定時間経過したので終了する
                    context.Performed();
                }
                else if (context.ControlIsActuated(pressPointOrDefault))
                {
                    // 再び入力があった
                    // 一定時間経過していないので、継続とみなす
                    _currentTapPhase = TapPhase.WaitingForRelease;
                    //context.PerformedAndStayPerformed();
                }

                break;
        }
    }

    /// <summary>
    /// Interactionの状態リセット
    /// </summary>
    public void Reset()
    {
        _currentTapPhase = TapPhase.None;
        _currentTapStartTime = 0;
        _lastTapReleaseTime = 0;
        _currentTapCount = 0;
    }
}
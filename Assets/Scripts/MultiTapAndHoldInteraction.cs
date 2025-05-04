using UnityEngine;
using UnityEngine.InputSystem;

internal class MultiTapAndHoldInteraction : IInputInteraction<float>
{
    // �ő�̃^�b�v����[s]
    public float tapTime;

    // ���̃^�b�v�܂ł̍ő�ҋ@����[s]
    public float tapDelay;

    // �K�v�ȃ^�b�v��
    public int tapCount = 2;

    // ���͔����臒l(0�Ńf�t�H���g�l)
    public float pressPoint;

    // �����[�X�����臒l(0�Ńf�t�H���g�l)
    public float releasePoint;

    // �}���`�^�b�v���z�[���h��A���͂��Ȃ��Ȃ��Ă���I������܂ł̎���
    public float endDelay;

    // �^�b�v��Ԃ̓����t�F�[�Y
    private enum TapPhase
    {
        None,
        WaitingForNextRelease,
        WaitingForNextPress,
        WaitingForRelease,
        WaitingForEnd,
    }

    // �ݒ�l���f�t�H���g�l�̒l���i�[����t�B�[���h
    private float tapTimeOrDefault => tapTime > 0.0 ? tapTime : InputSystem.settings.defaultTapTime;
    private float tapDelayOrDefault => tapDelay > 0.0 ? tapDelay : InputSystem.settings.multiTapDelayTime;
    private float pressPointOrDefault => pressPoint > 0 ? pressPoint : InputSystem.settings.defaultButtonPressPoint;
    private float releasePointOrDefault => pressPointOrDefault * InputSystem.settings.buttonReleaseThreshold;

    // Interaction�̓������
    private TapPhase _currentTapPhase = TapPhase.None;
    private double _currentTapStartTime;
    private double _lastTapReleaseTime;
    private int _currentTapCount;

    private double _lastTapReleaseTime_contious;

    // �Ō�̓��͂��}���`�^�b�v�̖������ǂ���
    private bool lastTapAccepted = false;
    
    /// <summary>
    /// ������
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    public static void Initialize()
    {
        // �����Interaction��o�^����K�v������
        InputSystem.RegisterInteraction<MultiTapAndHoldInteraction>();
    }

    /// <summary>
    /// Interaction�̓�������
    /// </summary>
    public void Process(ref InputInteractionContext context)
    {
        // �^�C���A�E�g����
        /*if (context.timerHasExpired)
        {
            // �ő勖�e���Ԃ𒴂��ă^�C���A�E�g�ɂȂ����ꍇ�̓L�����Z��
            context.Canceled();
            Debug.Log("timeout");
            return;
        }*/

        switch (_currentTapPhase)
        {
            case TapPhase.None:
                // �������

                // �^�b�v���ꂽ���`�F�b�N
                if (context.ControlIsActuated(pressPointOrDefault))
                {
                    _currentTapStartTime = context.time;

                    // �Ō�̓��͂̌p�����ǂ�������
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
                        // �K�v�ȃ^�b�v���ɒB������Performed�R�[���o�b�N���s
                        _currentTapPhase = TapPhase.WaitingForRelease;
                        context.Started();
                        lastTapAccepted = true;
                        //context.PerformedAndStayPerformed();
                    }
                    else
                    {
                        // ���͂��Ȃ��Ȃ�܂őҋ@
                        _currentTapPhase = TapPhase.WaitingForNextRelease;
                        //context.Started();
                        context.SetTimeout(tapTimeOrDefault);
                    }
                }
                break;

            case TapPhase.WaitingForNextRelease:
                // ���͂��Ȃ��Ȃ�܂őҋ@���Ă�����
                if (!context.ControlIsActuated(releasePointOrDefault))
                {
                    if (context.time - _currentTapStartTime > tapTimeOrDefault)
                    {
                        // �ő勖�e���Ԃ𒴂����̂ŃL�����Z��
                        context.Canceled();
                        lastTapAccepted = false;
                        break;
                    }

                    // ���̓��͑҂���ԂɑJ��
                    _lastTapReleaseTime = context.time;
                    _currentTapPhase = TapPhase.WaitingForNextPress;
                    context.SetTimeout(tapDelayOrDefault);
                }

                break;

            case TapPhase.WaitingForNextPress:
                // ���̓��͑҂��̏��
                if (context.ControlIsActuated(pressPointOrDefault))
                {
                    if (context.time - _lastTapReleaseTime > tapDelayOrDefault)
                    {
                        // �ő勖�e���Ԃ𒴂����̂ŃL�����Z��
                        context.Canceled();
                        lastTapAccepted = false;
                        break;
                    }

                    ++_currentTapCount;
                    _currentTapStartTime = context.time;

                    if (_currentTapCount >= tapCount)
                    {
                        // �K�v�ȃ^�b�v���ɒB�����̂ŁAPerformed�R�[���o�b�N�ʒm
                        // �I���܂őҋ@�����ԂɑJ��
                        _currentTapPhase = TapPhase.WaitingForRelease;
                        context.Started();
                        lastTapAccepted = true;
                        //context.PerformedAndStayPerformed();
                    }
                    else
                    {
                        // �K�v�^�b�v���ɒB���Ă��Ȃ��̂ŁA���͂��Ȃ��Ȃ�܂őҋ@
                        _currentTapPhase = TapPhase.WaitingForNextRelease;
                        context.SetTimeout(tapTimeOrDefault);
                    }

                    _currentTapStartTime = context.time;
                }

                break;

            case TapPhase.WaitingForRelease:
                // �}���`�^�b�v�����A���͂��Ȃ��Ȃ�܂őҋ@���Ă�����

                // ���̓`�F�b�N
                if (!context.ControlIsActuated(releasePointOrDefault))
                {
                    // ���͂��Ȃ��Ȃ����̂ŏI��
                    _currentTapPhase = TapPhase.WaitingForEnd;
                    _lastTapReleaseTime = context.time;
                    context.SetTimeout(endDelay);
                }

                break;

            case TapPhase.WaitingForEnd:
                // ���͂��Ȃ��Ȃ��Ă���Interaction���I������܂őҋ@���Ă�����
                if (context.time - _lastTapReleaseTime >= endDelay)
                {
                    _lastTapReleaseTime_contious = Time.time;

                    // ��莞�Ԍo�߂����̂ŏI������
                    context.Performed();
                }
                else if (context.ControlIsActuated(pressPointOrDefault))
                {
                    // �Ăѓ��͂�������
                    // ��莞�Ԍo�߂��Ă��Ȃ��̂ŁA�p���Ƃ݂Ȃ�
                    _currentTapPhase = TapPhase.WaitingForRelease;
                    //context.PerformedAndStayPerformed();
                }

                break;
        }
    }

    /// <summary>
    /// Interaction�̏�ԃ��Z�b�g
    /// </summary>
    public void Reset()
    {
        _currentTapPhase = TapPhase.None;
        _currentTapStartTime = 0;
        _lastTapReleaseTime = 0;
        _currentTapCount = 0;
    }
}
NetworkOperation
=====

Watermelona를 이용하여 네트워크 작업을 처리하는 예제

네트워크 작업의 분류
----
```c#
public enum RetryPolicy
{
    Ignore,
    
    /// <summary>
    /// 전송 실패 시, 재시도해도 지장 없음
    /// </summary>
    Retryable,

    /// <summary>
    /// 전송 실패 시, 중단해야 함
    /// </summary>
    Fatal
}
```

메세지 모델링
----
```c#
public class NetOperationFailed
{
    public object packet { get; set; }
    public object callback { get; set; }
        
    public Network.RetryPolicy retryPolicy { get; set; }
}
public class UnrecoverableError
{
}
```

namespace CQ.ApiElements;

/// <summary>
/// Scheme of the Authorization header, the word before the token:
/// <c>Authorization: &lt;scheme&gt; &lt;token&gt;</c>. Matched by name, ignoring
/// case.
/// </summary>
public enum AuthorizationType
{
    Bearer,

    Subscription,

    Key
}

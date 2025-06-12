namespace Jira.Api;

/// <summary>
/// Represents the values of a cascading select list custom field.
/// </summary>
/// <remarks>
/// Creates a new instance of a CascadingSelectCustomField.
/// </remarks>
/// <param name="name">The name of the custom field.</param>
/// <param name="parentOption">The value of the parent option.</param>
/// <param name="childOption">The value of the child option.</param>
public class CascadingSelectCustomField(string name, string parentOption, string childOption)
{
	private readonly string _name = name;
	private readonly string _parentOption = parentOption;
	private readonly string _childOption = childOption;

	/// <summary>
	/// The name of this custom field.
	/// </summary>
	public string Name
	{
		get { return _name; }
	}

	/// <summary>
	/// The value of the parent option.
	/// </summary>
	public string ParentOption
	{
		get { return _parentOption; }
	}

	/// <summary>
	/// The value of the child option.
	/// </summary>
	public string ChildOption
	{
		get { return _childOption; }
	}
}

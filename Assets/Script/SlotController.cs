using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
	[SerializeField] SlotConfig config;
	[SerializeField] SlotView view;
	[SerializeField] Button spinButton;
	private SlotModel model;

	void Start()
	{
		model = new SlotModel(config);
		view.Initialize(model);
		spinButton.onClick.AddListener(OnSpinButtonClicked);
	}

	public void OnSpinButtonClicked()
	{
		if (model.Credits >= model.Bet)
		{
			model.Spin();
		}
	}
}
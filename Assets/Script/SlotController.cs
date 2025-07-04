using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
	[SerializeField] SlotConfig config;
	[SerializeField] SlotView slotView;
	[SerializeField] Button spinButton;
	SlotModel slotModel;

	void Start()
	{
		slotModel = new SlotModel();
		slotModel.Initialize(config);
		slotView.Initialize(slotModel);
		spinButton.onClick.AddListener(OnSpin);
	}

	void OnSpin()
	{
		slotModel.Spin();
	}
}
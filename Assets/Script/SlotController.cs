using UnityEngine;

public class SlotController : MonoBehaviour
{
    [SerializeField] SlotView slotView;
    [SerializeField] SlotConfig config;
    [SerializeField] int rows = 3; // 預設 3 行
    [SerializeField] int columns = 3; // 預設 3 列
    SlotModel slotModel = new SlotModel();

    void Start()
    {
        slotModel.Initialize(config, rows, columns);
        slotView.Initialize(config, rows, columns);
        slotModel.OnGridUpdated += () => slotView.UpdateGrid(slotModel.Grid);
        slotModel.OnScoreUpdated += score => slotView.UpdateScore(score);
        slotView.SetSpinButton(OnSpin);
        slotView.UpdateScore(slotModel.Score);
        slotView.UpdateGrid(slotModel.Grid);
    }

    void OnSpin()
    {
        slotModel.Spin();
    }
}
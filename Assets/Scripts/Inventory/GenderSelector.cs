using UnityEngine;

public class GenderSelector : MonoBehaviour
{
    // Biến tĩnh để lưu giới tính, tồn tại xuyên suốt các Scene
    public static bool SelectedIsFemale = false;

    public void SelectMale()
    {
        SelectedIsFemale = false;
        //Debug.Log("[GenderSelector] Đã chọn: NAM");
    }

    public void SelectFemale()
    {
        SelectedIsFemale = true;
        //Debug.Log("[GenderSelector] Đã chọn: NỮ");
    }
}
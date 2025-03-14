using UnityEngine;

public class GrowTreeControl : MonoBehaviour
{
    public GameObject TreeOne, TreeTwo, TreeThree, TreeFour,TreeGroupe;
    private int currentScore = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
        TreeOne.SetActive(true);
        TreeTwo.SetActive(false);
        TreeThree.SetActive(false);
        TreeFour.SetActive(false);
        TreeGroupe.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        currentScore = ScoreSystem.Instance.GetCurrentScore();
        if (currentScore >1 )
        {
            TreeOne.SetActive(false);
            TreeTwo.SetActive(true);
            TreeThree.SetActive(false);
            TreeFour.SetActive(false);
        }
        if (currentScore > 3)
        {
            TreeOne.SetActive(false);
            TreeTwo.SetActive(false);
            TreeThree.SetActive(true);
            TreeFour.SetActive(false);
        }
        if (currentScore > 6)
        {
            TreeOne.SetActive(false);
            TreeTwo.SetActive(false);
            TreeThree.SetActive(false);
            TreeFour.SetActive(true);
        }
        if (currentScore > 8)
        {
            TreeGroupe.SetActive(true);
        }
    }
}

using UnityEngine;


public class Drawer : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject RegisterPanel;
    public GameObject ChangePassPanel;



    public void OpenChangePass()
    {
     
        ChangePassPanel.SetActive(true);
       
    }
    public void CloseChangePass()
    {
     
        ChangePassPanel.SetActive(false);
       
    }

    public void OpenLogin()
    {
     
        LoginPanel.SetActive(true);
       
    }

    public void OpenRegister()
    {
       
        RegisterPanel.SetActive(true);
       
    }

    public void CloseLogin()
    {
        LoginPanel.SetActive(false);  
    }

    public void CloseRegister()
    {
       RegisterPanel.SetActive(false); 
    }

    
}

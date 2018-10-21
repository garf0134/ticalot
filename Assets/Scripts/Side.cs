using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Side
{
  public enum Role
  {
    Human,
    AI
  }
  public Role role;
  public Color color;
  public string iconName;
  public string name;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }
}

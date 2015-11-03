using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

    public Sprite SpriteBuildable;
    public Sprite SpriteUnbuildable;

    private Sprite _currentSprite;
    private bool _buildable;
    private bool _walkable;
    private UnityEngine.Color _colorState;
    public Sprite mapSprite = null;

    public bool Buildable
    {
        set { _buildable = value; }
        get { return _buildable; }
    }

    public bool Walkable
    {
        set { _walkable = value; }
        get { return _walkable; }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (mapSprite != null)
        {
            _currentSprite = mapSprite;
            GetComponent<SpriteRenderer>().sprite = _currentSprite;
            GetComponent<SpriteRenderer>().color = UnityEngine.Color.white;
        }
        else if(_walkable && _currentSprite != SpriteBuildable)
        {
            _currentSprite = SpriteBuildable;
            GetComponent<SpriteRenderer>().sprite = _currentSprite;
        } else if (!_walkable && _currentSprite != SpriteUnbuildable)
        {
            _currentSprite = SpriteUnbuildable;
            GetComponent<SpriteRenderer>().sprite = _currentSprite;
        }

        if (_buildable && _colorState != UnityEngine.Color.green)
        {
            _colorState = UnityEngine.Color.green;
            GetComponent<SpriteRenderer>().color = _colorState;
        }
        else if (!_buildable && _colorState != UnityEngine.Color.red)
        {
            _colorState = UnityEngine.Color.red;
            GetComponent<SpriteRenderer>().color = _colorState;
        }

	}
}

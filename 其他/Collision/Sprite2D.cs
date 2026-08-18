using Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Sprite2D : CObject
{
    public SpriteRenderer sprite;
    public bool isHit;

    private void Start()
    {
        sprite =  GetComponent<SpriteRenderer>();
    }

    public void setColor(Color color)
    {
        if (sprite != null)
            sprite.color = color;
    }

    void Update()
    {
        setColor(isHit ? Color.green : Color.white);
        isHit = false;
    }

    public override void OnTrigger(CBody b, Trigger trigger)
    {
        bool isHit = trigger != Trigger.Exit;
        if (isHit && !this.isHit)
        {
            this.isHit = isHit;
        }
    }
}


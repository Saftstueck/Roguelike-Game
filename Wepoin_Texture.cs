using Godot;

public partial class Wepoin_Texture : Node2D
{
    public void SetWeaponTexture(Texture2D texture)
    {
        Sprite2D sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        if (sprite == null)
        {
            sprite = new Sprite2D { Name = "Sprite2D" };
            AddChild(sprite);
        }

        sprite.Texture = texture;
    }
}
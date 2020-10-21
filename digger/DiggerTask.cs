using System.Drawing.Text;
using System.Windows.Forms;

namespace Digger
{
    public class Player : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand();

            switch (Game.KeyPressed)
            {
                case Keys.Right:
                    command.DeltaX = 1;
                    break;
                case Keys.Left:
                    command.DeltaX = -1;
                    break;
                case Keys.Up:
                    command.DeltaY = -1;
                    break;
                case Keys.Down:
                    command.DeltaY = 1;
                    break;
                default:
                    Stay();
                    break;
            }

            if (IsBorderNear(x, y, command) || IsSackNear(x, y, command))
                return Stay();

            return command;
        }

        private static bool IsSackNear(int x, int y, CreatureCommand command)
        {
            var potentialSack = Game.Map[x + command.DeltaX, y + command.DeltaY];

            return potentialSack != null && potentialSack.ToString() == "Digger.Sack";
        }

        private static bool IsBorderNear(int x, int y, CreatureCommand command)
        {
            return
                x + command.DeltaX < 0 ||
                x + command.DeltaX >= Game.MapWidth ||
                y + command.DeltaY < 0 ||
                y + command.DeltaY >= Game.MapHeight;
        }

        private CreatureCommand Stay()
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            var conflictedObjectName = conflictedObject.ToString();
            if (conflictedObjectName == "Digger.Gold")
                Game.Scores += 10;
            return conflictedObjectName == "Digger.Sack" || conflictedObjectName == "Digger.Monster";
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public string GetImageFileName()
        {
            return "Digger.png";
        }
    }

    public class Terrain : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Terrain.png";
        }
    }

    public class Sack : ICreature
    {
        private int fieldsFell = 0;
        public CreatureCommand Act(int x, int y)
        {
            if (y + 1 < Game.MapHeight)
            {
                var mapField = Game.Map[x, y + 1];
                if (mapField == null || (fieldsFell > 0
                    && (mapField.ToString() == "Digger.Player" ||
                    mapField.ToString() == "Digger.Monster")))
                {
                    fieldsFell++;
                    return Fall();
                }
            }

            if (fieldsFell > 1)
            {
                fieldsFell = 0;
                return TransformToGold();
            }

            fieldsFell = 0;
            return Stay();
        }

        private CreatureCommand Fall()
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 1 };
        }

        private CreatureCommand TransformToGold()
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() };
        }

        private CreatureCommand Stay()
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int GetDrawingPriority()
        {
            return 5;
        }

        public string GetImageFileName()
        {
            return "Sack.png";
        }
    }

    public class Gold : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
        }

        public int GetDrawingPriority()
        {
            return 4;
        }

        public string GetImageFileName()
        {
            return "Gold.png";
        }
    }

    public class Monster : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand();

            if (Game.IsOver)
                return Stay();

            RunMonster(x, y, command);

            if (IsBorderNear(x, y, command))
                return Stay();

            var field = Game.Map[x + command.DeltaX, y + command.DeltaY];
            if (field != null)
            {
                if (IsObjectNear(field, "Digger.Terrain") ||
                IsObjectNear(field, "Digger.Sack") ||
                IsObjectNear(field, "Digger.Monster"))
                    return Stay();
            }

            return command;
        }

        private static void RunMonster(int x, int y, CreatureCommand command)
        {
            var playerField = GetPlayerField();
            if (playerField != null)
            {
                var pX = playerField[0];
                var pY = playerField[1];

                if (pY == y)
                {
                    if (pX > x) command.DeltaX = 1;
                    else if (pX < x) command.DeltaX = -1;
                }
                else if (pX == x)
                {
                    if (pY > y) command.DeltaY = 1;
                    else if (pY < y) command.DeltaY = -1;
                }
                else
                {
                    if (pX > x) command.DeltaX = 1;
                    else if (pX < x) command.DeltaX = -1;
                }
            }
        }

        private static int[] GetPlayerField()
        {
            for (var x = 0; x < Game.MapWidth; x++)
                for (var y = 0; y < Game.MapHeight; y++)
                    if (Game.Map[x, y] is Player)
                        return new int[] { x, y };
            return null;
        }

        private CreatureCommand Stay()
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }

        private static bool IsObjectNear(ICreature field, string obj)
        {
            return field.ToString() == obj;
        }

        private static bool IsBorderNear(int x, int y, CreatureCommand command)
        {
            return
                x + command.DeltaX < 0 ||
                x + command.DeltaX >= Game.MapWidth ||
                y + command.DeltaY < 0 ||
                y + command.DeltaY >= Game.MapHeight;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            var conflictedObjectName = conflictedObject.ToString();
            return conflictedObjectName == "Digger.Monster" || conflictedObjectName == "Digger.Sack";
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }
    }
}

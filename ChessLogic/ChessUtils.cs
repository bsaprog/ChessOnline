﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ChessLogic
{
    internal static class ChessUtils
    {
        internal static string GetAddresFromPosition(Vector2 position)
        {
            string result = "";

            int x = (Int32)position.X;
            int y = (Int32)position.Y;

            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                string[] conformity = new string[8]
                {
                    "a",
                    "b",
                    "c",
                    "d",
                    "e",
                    "f",
                    "g",
                    "h"
                };
                result = conformity[x] + (y + 1).ToString();
            }

            return result;
        }
        
        internal static Vector2 GetPositionFromAddress(string address)
        {
            Vector2 result = new Vector2(-1);

            if (address.Length == 2)
            {
                char x = address[0];
                Int32.TryParse(address[1].ToString(), out int y);

                if (x >= 'a' && x <= 'h' && y >= 1 && y <= 8)
                {
                    Dictionary<char, int> conformity = new Dictionary<char, int>
                    {
                        { 'a', 0 },
                        { 'b', 1 },
                        { 'c', 2 },
                        { 'd', 3 },
                        { 'e', 4 },
                        { 'f', 5 },
                        { 'g', 6 },
                        { 'h', 7 }
                    };

                    result = new Vector2(conformity[x], y - 1);
                }
            }

            return result;
        }

        internal static KnownColor InvertColor(KnownColor color)
        {
            KnownColor result;
            if (color == KnownColor.Black)
            {
                result = KnownColor.White;
            }
            else if (color == KnownColor.White)
            {
                result = KnownColor.Black;
            }
            else
            {
                result = color;
            }

            return result;
        }
    }
}

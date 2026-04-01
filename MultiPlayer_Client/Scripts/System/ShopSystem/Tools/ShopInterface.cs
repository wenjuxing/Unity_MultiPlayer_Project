using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IGoodsIconsService
{
    Task<Sprite> GetIconByIdAsync(int Id);
}
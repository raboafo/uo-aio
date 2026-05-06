namespace UOAIO;

public class DesignerGump : GEmpty
{
	private DesignContext m_Context;

	private DesignerGroup m_Group;

	private DesignerGroup m_SystemGroup;

	private Gump m_UpArrow;

	private Gump m_LeftArrow;

	private Gump m_RightArrow;

	private Gump m_UpArrowBack;

	private Gump m_Content;

	public DesignContext Context => this.m_Context;

	public DesignerGroup Group
	{
		get
		{
			return this.m_Group;
		}
		set
		{
			if (value != null && value.UseArrows)
			{
				value = value.Groups[0];
			}
			if (this.m_Group == value)
			{
				return;
			}
			this.m_Group = value;
			this.m_Content.Children.Clear();
			if (this.m_Group == null)
			{
				this.RemoveArrows();
				return;
			}
			int num = 0;
			int contentOffsetX = this.m_Group.ContentOffsetX;
			int contentOffsetY = this.m_Group.ContentOffsetY;
			int contentOffsetWidth = this.m_Group.ContentOffsetWidth;
			int contentOffsetHeight = this.m_Group.ContentOffsetHeight;
			int num2 = 0;
			while (num2 < this.m_Group.Groups.Length)
			{
				if (this.m_Group.Groups[num2] != null)
				{
					this.m_Content.Children.Add(new DesignerGroupItem(this, this.m_Group.Groups[num2], contentOffsetX + num % 8 * contentOffsetWidth, contentOffsetY + num / 8 * contentOffsetHeight, contentOffsetWidth, contentOffsetHeight));
				}
				num2++;
				num++;
			}
			int num3 = 0;
			while (num3 < this.m_Group.Entries.Length)
			{
				if (this.m_Group.Entries[num3] != null)
				{
					this.m_Content.Children.Add(new DesignerEntryItem(this, this.m_Group.Entries[num3], contentOffsetX + num % 8 * contentOffsetWidth, contentOffsetY + num / 8 * contentOffsetHeight, contentOffsetWidth, contentOffsetHeight));
				}
				num3++;
				num++;
			}
			if (contentOffsetHeight == 111)
			{
				int num4 = this.m_Content.Width - contentOffsetX - contentOffsetX;
				int num5 = 0;
				for (int i = 0; i < this.m_Content.Children.Count; i++)
				{
					if (this.m_Content.Children[i] is ItemButtonGump { Image: var image } itemButtonGump)
					{
						itemButtonGump.Y = itemButtonGump.Y + (contentOffsetHeight - (image.yMax - image.yMin + 1)) / 2 - image.yMin;
						num4 -= image.xMax - image.xMin + 1;
						num5++;
					}
				}
				num4 /= 7;
				int num6 = contentOffsetX;
				for (int j = 0; j < this.m_Content.Children.Count; j++)
				{
					if (this.m_Content.Children[j] is ItemButtonGump { Image: var image2 } itemButtonGump2)
					{
						itemButtonGump2.X = num6 - image2.xMin;
						num6 += num4 + (image2.xMax - image2.xMin + 1);
						if (itemButtonGump2 is DesignerEntryItem)
						{
							DesignerEntryItem designerEntryItem = (DesignerEntryItem)itemButtonGump2;
							designerEntryItem.ClipWidth = image2.xMax + 1 + num4;
						}
					}
				}
			}
			if (this.m_Group.Parent != null && this.m_Group.Parent.UseArrows)
			{
				this.AddArrows();
			}
			else
			{
				this.RemoveArrows();
			}
			if (this.m_Group == this.m_SystemGroup)
			{
				string[] array = new string[6] { "Backup", "Restore", "Synch", "Clear", "Commit", "Revert" };
				for (int k = 0; k < array.Length; k++)
				{
					this.m_Content.Children.Add(new DesignerSystemButton(this.m_Context, 29 + k / 2 * 120, 14 + k % 2 * 40, k, array[k]));
				}
			}
		}
	}

	public void AddArrows()
	{
		if (this.m_UpArrowBack == null)
		{
			base.m_Children.Add(this.m_UpArrowBack = new DesignerBackground(this.m_Context, 22003, 152, -17, 69, 54));
		}
		if (this.m_UpArrow == null)
		{
			base.m_Children.Add(this.m_UpArrow = new DesignerArrowButton(this, 0, 166, -12, 22050, 22051, 22052));
		}
		if (this.m_LeftArrow == null)
		{
			base.m_Children.Add(this.m_LeftArrow = new DesignerArrowButton(this, -1, 110, 46, 22053, 22054, 22055));
		}
		if (this.m_RightArrow == null)
		{
			base.m_Children.Add(this.m_RightArrow = new DesignerArrowButton(this, 1, 510, 46, 22056, 22057, 22058));
		}
	}

	public void RemoveArrows()
	{
		if (this.m_UpArrowBack != null)
		{
			Gumps.Destroy(this.m_UpArrowBack);
		}
		if (this.m_UpArrow != null)
		{
			Gumps.Destroy(this.m_UpArrow);
		}
		if (this.m_LeftArrow != null)
		{
			Gumps.Destroy(this.m_LeftArrow);
		}
		if (this.m_RightArrow != null)
		{
			Gumps.Destroy(this.m_RightArrow);
		}
		this.m_UpArrowBack = null;
		this.m_UpArrow = null;
		this.m_LeftArrow = null;
		this.m_RightArrow = null;
	}

	public void UpdateLevelButtons()
	{
		int currentLevel = this.m_Context.GetCurrentLevel();
		for (int i = 0; i < base.m_Children.Count; i++)
		{
			Gump gump = base.m_Children[i];
			if (gump is DesignerLevelButton)
			{
				DesignerLevelButton designerLevelButton = (DesignerLevelButton)gump;
				if (designerLevelButton.Level == currentLevel)
				{
					designerLevelButton.Activate();
				}
				else
				{
					designerLevelButton.Deactivate();
				}
			}
		}
	}

	public DesignerGump(DesignContext context)
		: base(30, 30, 639, 154)
	{
		context.Multi.GetBounds(out var xMin, out var yMin, out var xMax, out var yMax);
		int num = xMax - xMin + 1;
		int num2 = yMax - yMin + 1;
		int num3 = ((num >= 14 || num2 >= 14) ? 4 : 3);
		this.m_Context = context;
		base.m_NonRestrictivePicking = true;
		this.m_Content = new DesignerBackground(context, 3604, 121, 19, 397, 120);
		base.m_Children.Add(this.m_Content);
		base.m_Children.Add(new DesignerBackground(context, 22000, 0, 0, 153, 154));
		base.m_Children.Add(new DesignerBackground(context, 22001, 153, 0, 333, 154));
		if (num3 == 4)
		{
			base.m_Children.Add(new DesignerBackground(context, 22002, 486, 0, 153, 154));
		}
		else
		{
			base.m_Children.Add(new DesignerBackground(context, 22009, 486, 0, 153, 154));
		}
		int num4 = ((num3 == 4) ? 35 : 51);
		int num5 = num3;
		base.m_Children.Add(new DesignerLevelButton(context, num5--, 623, num4, 22250, 22251, 22252, 22253, 22254, 22255));
		num4 += 18;
		base.m_Children.Add(new DesignerLevelButton(context, num5--, 623, num4, 22256, 22257, 22258, 22259, 22260, 22261));
		num4 += 16;
		if (num3 == 4)
		{
			base.m_Children.Add(new DesignerLevelButton(context, num5--, 623, num4, 22256, 22257, 22258, 22259, 22260, 22261));
			num4 += 16;
		}
		base.m_Children.Add(new DesignerLevelButton(context, num5--, 623, num4, 22262, 22263, 22264, 22265, 22266, 22267));
		base.m_Children.Add(new DesignerDeleteButton(this, 9, 83));
		base.m_Children.Add(new DesignerPickButton(this, 39, 83));
		DesignerGroup designerGroup = new DesignerGroup(0)
		{
			ContentOffsetX = 13,
			ContentOffsetY = 8,
			ContentOffsetWidth = 47,
			ContentOffsetHeight = 61
		};
		DesignerGroup designerGroup2 = designerGroup;
		designerGroup2 = designerGroup.AddGroup(1193);
		designerGroup2.AddBorderedFloorSet(new int[4] { 1193, 1194, 1195, 1196 }, 1204, 1200, 1203, 1199, 1201, 1198, 1202, 1197, 1189, 1190);
		designerGroup2.AddBorderedFloorSet(new int[4] { 1205, 1206, 1207, 1208 }, 1216, 1212, 1215, 1211, 1213, 1210, 1214, 1209, 1191, 1192);
		designerGroup2.AddBorderedFloorSet(new int[6] { 1222, 1223, 1224, 1225, 1225, 1225 }, 1233, new int[2] { 1227, 1228 }, 1235, 1230, 1234, new int[2] { 1226, 1229 }, 1232, 1231, 1185, 1186);
		designerGroup2.AddBorderedFloorSet(new int[6] { 1236, 1237, 1238, 1239, 1239, 1239 }, 1247, new int[2] { 1241, 1242 }, 1249, 1244, 1248, new int[2] { 1240, 1243 }, 1246, 1239, 1187, 1188);
		designerGroup2 = designerGroup.AddGroup(1305);
		designerGroup2.AddTiledFloorSet(new int[4] { 1305, 1306, 1307, 1308 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1309, 1310, 1311, 1312 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1313, 1314, 1315, 1316 });
		designerGroup2.AddTiledFloorSet(new int[2] { 1181, 1183 });
		designerGroup2.AddTiledFloorSet(new int[2] { 1182, 1184 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1181, 1182, 1183, 1184 });
		designerGroup2 = designerGroup.AddGroup(1317);
		designerGroup2.AddTiledFloorSet(new int[4] { 1317, 1318, 1319, 1320 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1321, 1322, 1323, 1324 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1327, 1328, 1329, 1330 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1331, 1332, 1333, 1334 });
		designerGroup2 = designerGroup.AddGroup(1280);
		designerGroup2.AddTiledFloorSet(new int[4] { 1280, 1281, 1282, 1283 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1276, 1277, 1278, 1279 });
		designerGroup2.AddTiledFloorSet(new int[7] { 1407, 1408, 1409, 1410, 1411, 1412, 1413 });
		designerGroup2.AddTiledFloorSet(new int[12]
		{
			1250, 1251, 1252, 1253, 1254, 1255, 1256, 1257, 1335, 1336,
			1337, 1338
		});
		designerGroup2 = designerGroup.AddGroup(1258);
		designerGroup2.AddIndexedFloorSet(1258, 1259, 1260, 1261, 1262, 1263, 1264, 1265, 1266);
		designerGroup2.AddIndexedFloorSet(1267, 1268, 1269, 1270, 1271, 1272, 1273, 1274, 1275);
		designerGroup2 = designerGroup.AddGroup(1293);
		designerGroup2.AddTiledFloorSet(new int[2] { 1293, 1294 });
		designerGroup2.AddTiledFloorSet(new int[2] { 1295, 1296 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1297, 1298, 1299, 1300 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1173, 1174, 1175, 1176 });
		designerGroup2.AddTiledFloorSet(1179);
		designerGroup2.AddTiledFloorSet(1180);
		designerGroup2 = designerGroup.AddGroup(1395);
		designerGroup2.AddTiledFloorSet(new int[4] { 1395, 1396, 1397, 1398 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1401, 1402, 1403, 1406 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1595, 1596, 1597, 1598 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1373, 1374, 1375, 1376 });
		designerGroup2 = designerGroup.AddGroup(1284);
		designerGroup2.AddTiledFloorSet(1288);
		designerGroup2.AddTiledFloorSet(1284);
		designerGroup2.AddTiledFloorSet(1286);
		designerGroup2.AddTiledFloorSet(1287);
		designerGroup2.AddTiledFloorSet(1285);
		designerGroup2.AddEntry(null);
		designerGroup2.AddTiledFloorSet(1289);
		designerGroup2.AddTiledFloorSet(1290);
		designerGroup2.AddTiledFloorSet(1220);
		designerGroup2.AddTiledFloorSet(1218);
		designerGroup2.AddTiledFloorSet(1217);
		designerGroup2.AddTiledFloorSet(1219);
		designerGroup2.AddTiledFloorSet(1221);
		designerGroup2.AddEntry(null);
		designerGroup2.AddTiledFloorSet(1291);
		designerGroup2.AddTiledFloorSet(1292);
		designerGroup2 = designerGroup.AddGroup(1339);
		designerGroup2.AddBorderedFloorSet(new int[5] { 1339, 1340, 1341, 1342, 1343 }, 1044, 1035, 1046, 1036, 1043, 1038, 1045, 1037);
		designerGroup2.AddBorderedFloorSet(new int[4] { 12788, 12789, 12790, 12791 }, 1039, 1038, 1042, 1037, 1040, 1035, 1041, 1036);
		designerGroup2 = designerGroup.AddGroup(6013);
		designerGroup2.AddTiledFloorSet(new int[5] { 6013, 6014, 6015, 6016, 6017 });
		designerGroup2.AddTiledFloorSet(new int[4] { 6077, 6078, 6079, 6080 });
		designerGroup2.AddTiledFloorSet(new int[4] { 12788, 12789, 12790, 12791 });
		designerGroup2.AddTiledFloorSet(new int[4] { 12792, 12793, 12794, 12795 });
		designerGroup2.AddTiledFloorSet(new int[4] { 1301, 1302, 1303, 1304 });
		designerGroup2.AddTiledFloorSet(new int[6] { 6039, 6040, 6041, 6042, 6043, 6044 });
		designerGroup2.AddBorderedFloorSet(new int[7] { 4850, 4862, 4868, 4874, 4880, 4886, 4892 }, 4936, new int[2] { 4921, 4924 }, 4906, new int[2] { 4894, 4897 }, 4900, new int[3] { 4912, 4915, 4918 }, 4933, new int[2] { 4927, 4930 });
		designerGroup2.AddBorderedFloorSet(new int[6] { 12906, 3289707, 12908, 12909, 12910, 12911 }, new int[2] { 12888, 12809 }, 12897, new int[2] { 12901, 12904 }, new int[2] { 12900, 12901 }, 12902, new int[3] { 12894, 12895, 12896 }, 12810, new int[3] { 12898, 12899, 12903 });
		base.m_Children.Add(new DesignerGroupButton(this, designerGroup, 70, 23, 22106));
		int[][][] array = new int[16][][]
		{
			new int[3][]
			{
				new int[12]
				{
					10, 7, 12, 6, 13, 8, 11, 9, 0, 14,
					15, 0
				},
				new int[12]
				{
					18, 18, 18, 16, 17, 17, 17, 19, 0, 0,
					0, 0
				},
				new int[12]
				{
					22, 22, 22, 20, 21, 21, 21, 23, 0, 0,
					0, 0
				}
			},
			new int[4][]
			{
				new int[12]
				{
					171, 168, 173, 166, 172, 167, 170, 169, 0, 186,
					185, 0
				},
				new int[12]
				{
					178, 175, 181, 174, 180, 176, 179, 177, 0, 188,
					187, 0
				},
				new int[12]
				{
					948, 948, 948, 947, 949, 949, 949, 950, 0, 0,
					0, 0
				},
				new int[12]
				{
					191, 191, 191, 189, 190, 190, 190, 192, 0, 0,
					0, 0
				}
			},
			new int[2][]
			{
				new int[12]
				{
					30, 28, 33, 26, 32, 27, 31, 29, 0, 34,
					35, 0
				},
				new int[12]
				{
					37, 37, 37, 36, 38, 38, 38, 39, 0, 0,
					0, 0
				}
			},
			new int[4][]
			{
				new int[12]
				{
					464, 464, 464, 463, 465, 465, 465, 466, 0, 467,
					468, 0
				},
				new int[12]
				{
					489, 489, 489, 488, 490, 490, 490, 491, 0, 0,
					0, 0
				},
				new int[12]
				{
					200, 200, 200, 199, 201, 201, 201, 204, 0, 202,
					203, 0
				},
				new int[12]
				{
					222, 222, 222, 220, 221, 221, 221, 223, 0, 0,
					0, 0
				}
			},
			new int[3][]
			{
				new int[12]
				{
					55, 52, 57, 51, 58, 53, 56, 54, 0, 59,
					60, 0
				},
				new int[12]
				{
					62, 62, 62, 61, 63, 63, 63, 64, 0, 0,
					0, 0
				},
				new int[12]
				{
					66, 66, 66, 65, 67, 67, 67, 68, 0, 0,
					0, 0
				}
			},
			new int[4][]
			{
				new int[12]
				{
					88, 88, 88, 89, 87, 87, 87, 90, 92, 94,
					93, 91
				},
				new int[12]
				{
					95, 95, 95, 97, 96, 96, 96, 98, 0, 0,
					0, 0
				},
				new int[12]
				{
					99, 99, 99, 101, 100, 100, 100, 102, 0, 0,
					0, 0
				},
				new int[12]
				{
					105, 105, 105, 107, 106, 106, 106, 108, 0, 0,
					0, 0
				}
			},
			new int[4][]
			{
				new int[12]
				{
					444, 444, 440, 438, 439, 445, 445, 441, 0, 452,
					453, 0
				},
				new int[12]
				{
					446, 450, 448, 0, 449, 451, 447, 441, 0, 454,
					455, 0
				},
				new int[12]
				{
					427, 422, 423, 421, 426, 425, 428, 424, 429, 430,
					432, 431
				},
				new int[12]
				{
					8539, 8539, 8539, 8540, 8538, 8538, 8538, 424, 0, 0,
					0, 0
				}
			},
			new int[2][]
			{
				new int[12]
				{
					149, 146, 151, 144, 150, 145, 148, 147, 0, 152,
					153, 0
				},
				new int[12]
				{
					159, 156, 161, 154, 160, 155, 158, 157, 0, 0,
					0, 0
				}
			},
			new int[3][]
			{
				new int[12]
				{
					552, 552, 552, 550, 551, 551, 551, 553, 0, 0,
					0, 0
				},
				new int[12]
				{
					1072, 1058, 546, 547, 1057, 545, 545, 553, 0, 0,
					0, 0
				},
				new int[12]
				{
					1059, 1059, 1059, 1061, 1060, 1060, 1060, 0, 0, 0,
					0, 0
				}
			},
			new int[7][]
			{
				new int[12]
				{
					249, 249, 249, 248, 250, 250, 250, 251, 0, 252,
					253, 0
				},
				new int[12]
				{
					255, 255, 255, 254, 256, 256, 256, 257, 0, 258,
					259, 0
				},
				new int[12]
				{
					261, 261, 261, 260, 262, 262, 262, 263, 0, 264,
					265, 0
				},
				new int[12]
				{
					267, 267, 267, 266, 268, 268, 268, 269, 0, 0,
					0, 0
				},
				new int[12]
				{
					271, 271, 271, 270, 272, 272, 272, 273, 0, 0,
					0, 0
				},
				new int[12]
				{
					1091, 1091, 1091, 1090, 1092, 1092, 1092, 1093, 0, 0,
					0, 0
				},
				new int[12]
				{
					280, 280, 280, 279, 281, 281, 281, 282, 0, 0,
					0, 0
				}
			},
			new int[7][]
			{
				new int[12]
				{
					670, 670, 670, 669, 671, 671, 671, 672, 0, 686,
					685, 0
				},
				new int[12]
				{
					664, 664, 664, 663, 665, 665, 665, 666, 0, 667,
					668, 0
				},
				new int[12]
				{
					658, 658, 658, 657, 659, 659, 659, 660, 0, 661,
					662, 0
				},
				new int[12]
				{
					674, 674, 674, 673, 675, 675, 675, 676, 0, 0,
					0, 0
				},
				new int[12]
				{
					698, 698, 698, 697, 699, 699, 699, 700, 0, 0,
					0, 0
				},
				new int[12]
				{
					1105, 1105, 1105, 1104, 1106, 1106, 1106, 1107, 0, 0,
					0, 0
				},
				new int[12]
				{
					694, 694, 694, 693, 695, 695, 695, 696, 0, 0,
					0, 0
				}
			},
			new int[5][]
			{
				new int[12]
				{
					345, 345, 345, 344, 346, 346, 346, 347, 0, 348,
					349, 0
				},
				new int[12]
				{
					352, 352, 352, 350, 351, 351, 351, 353, 0, 355,
					354, 0
				},
				new int[12]
				{
					357, 357, 357, 356, 358, 358, 358, 359, 0, 0,
					0, 0
				},
				new int[12]
				{
					362, 362, 362, 360, 361, 361, 361, 363, 0, 0,
					0, 0
				},
				new int[12]
				{
					517, 515, 512, 511, 513, 516, 518, 514, 519, 521,
					520, 522
				}
			},
			new int[4][]
			{
				new int[12]
				{
					598, 598, 598, 597, 599, 599, 599, 601, 0, 0,
					0, 0
				},
				new int[12]
				{
					592, 592, 592, 591, 593, 593, 593, 600, 0, 0,
					0, 0
				},
				new int[12]
				{
					595, 595, 595, 594, 596, 596, 596, 602, 0, 0,
					0, 0
				},
				new int[12]
				{
					589, 589, 589, 588, 590, 590, 590, 603, 0, 0,
					0, 0
				}
			},
			new int[6][]
			{
				new int[12]
				{
					968, 968, 968, 967, 969, 969, 969, 970, 0, 990,
					991, 0
				},
				new int[12]
				{
					972, 972, 972, 971, 973, 973, 973, 974, 0, 0,
					0, 0
				},
				new int[12]
				{
					983, 980, 993, 979, 994, 981, 984, 982, 0, 0,
					0, 0
				},
				new int[12]
				{
					983, 980, 993, 992, 994, 981, 984, 982, 0, 0,
					0, 0
				},
				new int[12]
				{
					960, 960, 960, 958, 961, 961, 961, 962, 0, 0,
					0, 0
				},
				new int[12]
				{
					976, 976, 976, 975, 977, 977, 977, 978, 0, 0,
					0, 0
				}
			},
			new int[6][]
			{
				new int[12]
				{
					312, 310, 307, 309, 308, 311, 313, 298, 0, 314,
					315, 0
				},
				new int[12]
				{
					312, 310, 307, 306, 308, 311, 313, 298, 0, 0,
					0, 0
				},
				new int[12]
				{
					302, 310, 296, 295, 297, 311, 303, 298, 0, 0,
					0, 0
				},
				new int[12]
				{
					304, 310, 300, 299, 301, 311, 305, 298, 0, 0,
					0, 0
				},
				new int[12]
				{
					336, 336, 336, 332, 334, 334, 334, 298, 0, 342,
					343, 0
				},
				new int[12]
				{
					338, 338, 338, 335, 339, 339, 339, 298, 0, 340,
					341, 0
				}
			},
			new int[5][]
			{
				new int[12]
				{
					910, 910, 910, 909, 911, 911, 911, 898, 0, 0,
					0, 0
				},
				new int[12]
				{
					912, 915, 907, 914, 908, 916, 913, 898, 0, 0,
					0, 0
				},
				new int[12]
				{
					912, 915, 907, 906, 908, 916, 913, 898, 0, 0,
					0, 0
				},
				new int[12]
				{
					902, 915, 896, 895, 897, 916, 903, 898, 0, 0,
					0, 0
				},
				new int[12]
				{
					904, 915, 900, 899, 901, 916, 905, 898, 0, 0,
					0, 0
				}
			}
		};
		DesignerGroup designerGroup3 = new DesignerGroup(0)
		{
			ContentOffsetX = 23,
			ContentOffsetY = 9,
			ContentOffsetWidth = 47,
			ContentOffsetHeight = 111
		};
		foreach (int[][] array2 in array)
		{
			DesignerGroup designerGroup4 = designerGroup3.AddGroup(array2[0][4]);
			designerGroup4.UseArrows = true;
			foreach (int[] array3 in array2)
			{
				designerGroup2 = designerGroup4.AddGroup(array3[0]);
				for (int k = 0; k < 8; k++)
				{
					designerGroup2.AddTiledFloorSet(array3[k]);
				}
			}
		}
		designerGroup3.ContentOffsetX = 3;
		designerGroup3.ContentOffsetY = 0;
		designerGroup3.ContentOffsetHeight = 61;
		base.m_Children.Add(new DesignerGroupButton(this, designerGroup3, 7, 24, 22100));
		DesignerGroup designerGroup5 = new DesignerGroup(0);
		designerGroup5.ContentOffsetX = 13;
		designerGroup5.ContentOffsetY = 8;
		designerGroup5.ContentOffsetWidth = 47;
		designerGroup5.ContentOffsetHeight = 61;
		designerGroup5.AddEntry(null);
		for (int l = 0; l < 6; l++)
		{
			designerGroup5.AddTiledFloorSet(6173 + l);
		}
		designerGroup5.AddEntry(null);
		designerGroup5.AddEntry(null);
		for (int m = 6; m < 12; m++)
		{
			designerGroup5.AddTiledFloorSet(6173 + m);
		}
		base.m_Children.Add(new DesignerGroupButton(this, designerGroup5, 39, 55, 22112));
		int[][][] array4 = new int[15][][]
		{
			new int[1][] { new int[8] { 44, 0, 41, 40, 42, 0, 43, 29 } },
			new int[4][]
			{
				new int[8] { 470, 0, 471, 469, 473, 0, 472, 466 },
				new int[8] { 476, 0, 477, 475, 479, 0, 478, 466 },
				new int[8] { 480, 481, 482, 483, 484, 485, 0, 0 },
				new int[8] { 474, 486, 487, 0, 0, 0, 0, 0 }
			},
			new int[4][]
			{
				new int[8] { 0, 16134, 9541, 9537, 9538, 9550, 9551, 9555 },
				new int[8] { 0, 9541, 9539, 9543, 9554, 9553, 9555, 0 },
				new int[8] { 0, 9555, 9548, 9549, 9550, 9544, 9541, 0 },
				new int[8] { 0, 9555, 9546, 9543, 9535, 9536, 9541, 0 }
			},
			new int[4][]
			{
				new int[8] { 209, 0, 207, 205, 206, 0, 208, 204 },
				new int[8] { 218, 0, 216, 212, 215, 0, 217, 204 },
				new int[8] { 225, 213, 225, 224, 214, 226, 227, 226 },
				new int[8] { 0, 211, 228, 219, 229, 210, 0, 0 }
			},
			new int[2][]
			{
				new int[8] { 71, 0, 72, 69, 70, 0, 73, 54 },
				new int[8] { 83, 80, 78, 82, 79, 81, 84, 0 }
			},
			new int[3][]
			{
				new int[8] { 111, 0, 112, 109, 110, 0, 113, 90 },
				new int[8] { 0, 118, 116, 115, 114, 117, 0, 119 },
				new int[8] { 631, 636, 633, 632, 634, 635, 641, 642 }
			},
			new int[5][]
			{
				new int[8] { 1082, 0, 1081, 1080, 1083, 0, 1084, 251 },
				new int[8] { 1087, 0, 1086, 1085, 1088, 0, 1089, 257 },
				new int[8] { 276, 0, 275, 274, 277, 0, 278, 263 },
				new int[8] { 284, 283, 285, 0, 287, 286, 288, 7978 },
				new int[8] { 0, 289, 290, 0, 292, 291, 0, 0 }
			},
			new int[5][]
			{
				new int[8] { 1096, 0, 1095, 1094, 1097, 0, 1098, 672 },
				new int[8] { 1101, 0, 1100, 1099, 1102, 0, 1103, 666 },
				new int[8] { 690, 0, 689, 688, 691, 0, 692, 660 },
				new int[8] { 680, 679, 681, 0, 683, 682, 684, 0 },
				new int[8] { 0, 701, 702, 0, 704, 703, 0, 0 }
			},
			new int[9][]
			{
				new int[8] { 368, 370, 365, 364, 366, 369, 367, 353 },
				new int[8] { 395, 394, 0, 396, 397, 0, 398, 399 },
				new int[8] { 0, 403, 402, 0, 400, 401, 0, 0 },
				new int[8] { 373, 374, 375, 376, 386, 387, 388, 389 },
				new int[8] { 420, 380, 378, 379, 377, 381, 371, 0 },
				new int[8] { 382, 383, 384, 385, 390, 391, 392, 393 },
				new int[8] { 405, 404, 406, 0, 0, 0, 0, 0 },
				new int[8] { 951, 952, 953, 954, 955, 956, 957, 0 },
				new int[8] { 959, 963, 964, 965, 966, 0, 0, 0 }
			},
			new int[2][]
			{
				new int[8] { 538, 536, 534, 535, 528, 530, 529, 531 },
				new int[8] { 538, 533, 529, 532, 528, 527, 534, 537 }
			},
			new int[5][]
			{
				new int[8] { 24, 25, 49, 50, 85, 86, 120, 121 },
				new int[8] { 456, 457, 162, 163, 164, 165, 193, 194 },
				new int[8] { 230, 231, 293, 294, 330, 331, 433, 434 },
				new int[8] { 435, 436, 409, 410, 494, 495, 407, 408 },
				new int[8] { 492, 493, 523, 524, 554, 555, 677, 678 }
			},
			new int[3][]
			{
				new int[8] { 711, 715, 714, 713, 716, 717, 712, 0 },
				new int[8] { 718, 724, 719, 721, 720, 722, 723, 725 },
				new int[8] { 0, 0, 726, 727, 728, 729, 0, 0 }
			},
			new int[9][]
			{
				new int[8] { 2083, 2082, 2081, 0, 2123, 2122, 2121, 0 },
				new int[8] { 2141, 2140, 2142, 2143, 2147, 2146, 2148, 2149 },
				new int[8] { 2226, 2227, 2228, 2229, 2243, 2244, 2245, 2246 },
				new int[8] { 0, 2230, 2231, 2232, 2233, 2234, 2235, 0 },
				new int[8] { 0, 2236, 2237, 2238, 2239, 2240, 2241, 2242 },
				new int[8] { 2285, 2283, 2284, 2286, 0, 0, 2299, 2300 },
				new int[8] { 2289, 2287, 2288, 2290, 0, 0, 2299, 2300 },
				new int[8] { 2294, 2291, 2292, 2293, 0, 0, 2299, 2300 },
				new int[8] { 2297, 2295, 2296, 2298, 0, 0, 2299, 2300 }
			},
			new int[2][]
			{
				new int[8] { 0, 13550, 13556, 13559, 13562, 13568, 13574, 0 },
				new int[8] { 0, 13582, 13586, 13592, 13598, 13604, 0, 0 }
			},
			new int[2][]
			{
				new int[8] { 0, 6425, 6424, 6419, 6427, 6426, 6417, 0 },
				new int[8] { 0, 6429, 6428, 6416, 6431, 6430, 6418, 0 }
			}
		};
		DesignerGroup designerGroup6 = new DesignerGroup(0)
		{
			ContentOffsetX = 23,
			ContentOffsetY = 9,
			ContentOffsetWidth = 47,
			ContentOffsetHeight = 111
		};
		foreach (int[][] array5 in array4)
		{
			DesignerGroup designerGroup7 = designerGroup6.AddGroup(array5[0][4]);
			designerGroup7.ContentOffsetX = 18;
			designerGroup7.UseArrows = true;
			foreach (int[] array6 in array5)
			{
				int num7 = 0;
				int num8 = 0;
				while (num7 == 0 && num8 < array6.Length)
				{
					num7 = array6[num8];
					num8++;
				}
				designerGroup2 = designerGroup7.AddGroup(num7);
				for (int num9 = 0; num9 < 8; num9++)
				{
					designerGroup2.AddTiledFloorSet(array6[num9]);
				}
			}
		}
		designerGroup6.ContentOffsetX = 3;
		designerGroup6.ContentOffsetY = 0;
		designerGroup6.ContentOffsetHeight = 61;
		base.m_Children.Add(new DesignerGroupButton(this, designerGroup6, 69, 55, 22115));
		this.m_SystemGroup = new DesignerGroup(0);
		base.m_Children.Add(new DesignerGroupButton(this, this.m_SystemGroup, 69, 83, 22124));
	}
}

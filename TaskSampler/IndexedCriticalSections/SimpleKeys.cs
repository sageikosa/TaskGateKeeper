using System;
using System.Collections.Generic;
using System.Text;

namespace TaskSampler.IndexedCriticalSections;

/// <summary>
/// Strong key to use with indexed customer critical sections
/// </summary>
/// <param name="ID"></param>
public readonly record struct CustomerID(long ID);

// parts of InventoryZoneKey
public readonly record struct ZoneID(long ID);
public readonly record struct ItemMasterID(long ID);

/// <summary>
/// Composite key to use with indexed inventory zone critical sections
/// </summary>
/// <param name="ZoneID">strong zone ID</param>
/// <param name="ItemMasterID">strong item master ID</param>
public readonly record struct InventoryZoneKey(ZoneID ZoneID, ItemMasterID ItemMasterID);
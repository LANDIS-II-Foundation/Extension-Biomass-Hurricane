'''
Based on the National Hurricane Center's hurricane database format for a
specific date/time/location. This module is part of the Hurdat2FC repository
by Paul Schrum.

This module reads a hurricane track database (HURDAT2 from www.nhc.noaa.gov)
and creates a new shapefile from it. If the shapefile already exists,
this module overwrites it. (It destroys the file if it fails during write.)

Contrary to the name, this module also creates a point shapefile from the
dataset.

The shapefiles are created in the Coordinate System is WGS83 and can not be
customized.

Usage:
python CreateHurricanePolylines.py <input file name> <output shapefile name>
'''

__author__ = 'Paul Schrum'

# To do List:
# 1. Create feature class (if path is gdb)
# 2. Store wind speed as z and barometric pressure as m.

import sys
if len(sys.argv) != 3:
    print("Failed to run because there are not enough arguments.")
    print()
    print("Usage:")
    print("python CreateHurricanePolylines.py <input file name> <out"
          "put shapefile name>")
    sys.exit(0)

import arcpy
import os

infile = sys.argv[1]
outfile_poly = sys.argv[2]

create_point_shapefile = True
if create_point_shapefile:
    pth, fname = os.path.split(outfile_poly)
    base, ext = os.path.splitext(fname)
    outfile_points = base + '_pts' +  ext
    outfile_points = os.path.join(pth, outfile_points)

if not os.path.exists(infile):
    if infile.upper() == 'Testing'.upper():
        infile = os.path.join(os.getcwd(), r"Test Data\hurdat2-2016-2018.txt")
    else:
        print("Can not find input file: {}".format(infile))
        print("No work performed. Exiting.")
        sys.exit(0)

if outfile_poly.upper() == 'Testing'.upper():
    outfile_poly = r'D:\Research\Datasets\Weather\Hurricanes\shapefiles' \
              r'\hurricanes.shp'

from HurricaneHistory import HurricaneHistory
history = HurricaneHistory(infile)

if not history:
    print("There was a problem loading the database file. Exiting.")
    sys.exit(0)

if arcpy.Exists(outfile_poly):
    arcpy.Delete_management(outfile_poly)

print("Input: {}".format(infile))
print("Output: {}".format(outfile_poly))

out_path, out_file_name = os.path.split(outfile_poly)
sr = arcpy.SpatialReference('Geographic Coordinate Systems/World/WGS 1984')
arcpy.CreateFeatureclass_management(out_path, out_file_name, "POLYLINE",
                                    None, has_m="ENABLED",
                                    has_z="ENABLED", spatial_reference=sr)

designation = 'Designatio'
name = 'Name'
unique_name = 'UniqueName'
start_time = 'StartTime'
end_time = 'EndTime'
max_wind = 'MaxWind'
min_pressure = 'MinPressur'
shape = 'SHAPE@'

field_list = [designation, name, unique_name, start_time, end_time, max_wind,
              min_pressure, shape]

field_dict = {}
for idx, item in enumerate(field_list):
    field_dict[item] = idx

arcpy.AddField_management(outfile_poly, field_name=designation,
                          field_type='TEXT', field_length=15, field_is_nullable='NON_NULLABLE')
arcpy.AddField_management(outfile_poly, field_name=name,
                          field_type='TEXT', field_length=15, field_is_nullable='NON_NULLABLE')
arcpy.AddField_management(outfile_poly, field_name=unique_name,
                          field_type='TEXT', field_length=31, field_is_nullable='NON_NULLABLE')
arcpy.AddField_management(outfile_poly, field_name=start_time,
                          field_type='DATE', field_is_nullable='NON_NULLABLE')
arcpy.AddField_management(outfile_poly, field_name=end_time,
                          field_type='DATE', field_is_nullable='NON_NULLABLE')
arcpy.AddField_management(outfile_poly, field_name=max_wind,
                          field_type='SHORT')
arcpy.AddField_management(outfile_poly, field_name=min_pressure,
                          field_type='SHORT')

from HurricaneTrack import HurricaneTrack

try:
    with arcpy.da.InsertCursor(outfile_poly, field_list) as cursor:
        a_storm: HurricaneTrack
        for a_storm in history.all_storms.values():
            print("Processing storm " + a_storm.unique_name)
            data_list = [None] * len(field_list)
            data_list[field_dict[designation]] = a_storm.designation
            data_list[field_dict[name]] = a_storm.name
            data_list[field_dict[unique_name]] = a_storm.unique_name
            data_list[field_dict[start_time]] = a_storm.start_time
            data_list[field_dict[end_time]] = a_storm.end_time
            data_list[field_dict[max_wind]] = a_storm.max_wind_speed
            data_list[field_dict[min_pressure]] = a_storm.min_bar_pressure
            line_points = [arcpy.Point(p[0], p[1], p[2], p[3]) for p in
                           a_storm.shape]
            line_points = arcpy.Array(line_points)
            data_list[field_dict[shape]] = arcpy.Polyline(line_points)
            cursor.insertRow(data_list)

finally:
    del cursor
    print("Input: {}".format(infile))
    print("Output: {}".format(outfile_points))

if create_point_shapefile:
    out_path, out_file_name = os.path.split(outfile_points)
    if arcpy.Exists(outfile_points):
        arcpy.Delete_management(outfile_points)

    print("Generating point shapefile: {}".format(out_file_name))
    arcpy.CreateFeatureclass_management(out_path, out_file_name, "POINT",
                                        None, has_m="DISABLED",
                                        has_z="ENABLED",
                                        spatial_reference=sr)

    designation = 'Designation'[:10]
    timestamp = 'TimeStamp'
    timestring = 'TimeString'
    identifier = 'Identifier'
    system_status = 'SystemStatus'[:10]
    max_sustained_winds = 'MaxSustWinds'[:10]
    min_pressure = 'MinPressure'[:10]
    age_minutes = 'AgeMinutes'
    ne34_radius = 'NE34RADIUS'
    se34_radius = 'SE34RADIUS'
    sw34_radius = 'SW34RADIUS'
    nw34_radius = 'NW34RADIUS'
    ne50_radius = 'NE50RADIUS'
    se50_radius = 'SE50RADIUS'
    sw50_radius = 'SW50RADIUS'
    nw50_radius = 'NW50RADIUS'
    ne64_radius = 'NE64RADIUS'
    se64_radius = 'SE64RADIUS'
    sw64_radius = 'SW64RADIUS'
    nw64_radius = 'NW64RADIUS'
    shapexy = 'SHAPE@XY'

    field_list = [designation, timestamp, timestring, identifier,
            system_status, max_sustained_winds, min_pressure, age_minutes,
            ne34_radius, se34_radius, sw34_radius, nw34_radius,
            ne50_radius, se50_radius, sw50_radius, nw50_radius,
            ne64_radius, se64_radius, sw64_radius, nw64_radius,
            shapexy]

    field_dict = {}
    for idx, item in enumerate(field_list):
        field_dict[item] = idx

    arcpy.AddField_management(outfile_points, field_name=designation,
                              field_type='TEXT', field_length=15,
                              field_is_nullable='NON_NULLABLE')
    arcpy.AddField_management(outfile_points, field_name=timestamp,
                              field_type='DATE',
                              field_is_nullable='NON_NULLABLE')
    arcpy.AddField_management(outfile_points, field_name=timestring,
                              field_type='TEXT', field_length=15)
    arcpy.AddField_management(outfile_points, field_name=identifier,
                              field_type='TEXT', field_length=3)
    arcpy.AddField_management(outfile_points, field_name=system_status,
                              field_type='TEXT', field_length=3)
    arcpy.AddField_management(outfile_points, field_name=max_sustained_winds,
                              field_type='SHORT')
    arcpy.AddField_management(outfile_points, field_name=min_pressure,
                              field_type='SHORT')
    arcpy.AddField_management(outfile_points, field_name=age_minutes,
                              field_type='FLOAT',
                              field_is_nullable='NON_NULLABLE')
    arcpy.AddField_management(outfile_points, field_name=ne34_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=se34_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=sw34_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=nw34_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=ne50_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=se50_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=sw50_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=nw50_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=ne64_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=se64_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=sw64_radius,
                              field_type='FLOAT')
    arcpy.AddField_management(outfile_points, field_name=nw64_radius,
                              field_type='FLOAT')

    from HurricaneRecord import HurricaneRecord

    try:
        with arcpy.da.InsertCursor(outfile_points, field_list) as cursor:
            a_storm: HurricaneTrack
            for a_storm in history.all_storms.values():
                # print("   Processing points for storm " + a_storm.unique_name)
                a_rec: HurricaneRecord
                for a_rec in a_storm.record_dict.values():
                    data_list = [None] * len(field_list)
                    data_list[field_dict[designation]] = a_storm.designation
                    data_list[field_dict[timestamp]] = a_rec.record_time
                    data_list[field_dict[timestring]] = \
                        a_rec.record_time.strftime('%Y%m%d, %H%M')
                    data_list[field_dict[identifier]] = a_rec.identifier
                    data_list[field_dict[system_status]] = \
                        a_rec.system_status
                    data_list[field_dict[max_sustained_winds]] = \
                        a_rec.max_sustained_wind
                    data_list[field_dict[min_pressure]] = a_rec.min_pressure
                    data_list[field_dict[age_minutes]] = a_rec.minutes
                    data_list[field_dict[ne34_radius]] = a_rec.ne34_radius
                    data_list[field_dict[se34_radius]] = a_rec.se34_radius
                    data_list[field_dict[sw34_radius]] = a_rec.sw34_radius
                    data_list[field_dict[nw34_radius]] = a_rec.nw34_radius
                    data_list[field_dict[ne50_radius]] = a_rec.ne50_radius
                    data_list[field_dict[se50_radius]] = a_rec.se50_radius
                    data_list[field_dict[sw50_radius]] = a_rec.sw50_radius
                    data_list[field_dict[nw50_radius]] = a_rec.nw50_radius
                    data_list[field_dict[ne64_radius]] = a_rec.ne64_radius
                    data_list[field_dict[se64_radius]] = a_rec.se64_radius
                    data_list[field_dict[sw64_radius]] = a_rec.sw64_radius
                    data_list[field_dict[nw64_radius]] = a_rec.nw64_radius

                    the_point = arcpy.Point(a_rec.coords[0], a_rec.coords[1],
                                            a_rec.coords[2], )
                    data_list[field_dict[shapexy]] = the_point
                    cursor.insertRow(data_list)

    finally:
        # del cursor
        print("Input: {}".format(infile))
        print("Output: {}".format(outfile_points))


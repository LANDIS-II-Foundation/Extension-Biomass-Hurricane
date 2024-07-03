'''
Based on the National Hurricane Center's hurricane database format for a
specific date/time/location. This module is part of the Hurdat2FC repository
by Paul Schrum.

Each instance of HurricaneRecord is one record from the database and includes,
among other things, the timestamp of the record, and the latitude and
longitude of the eye of the hurricane.
'''

import datetime

__author__ = 'Paul Schrum'

class HurricaneRecord:
    col_idx = {'date': 0,
               'time': 1,
               'ident': 2,
               'status': 3,
               'lat': 4,
               'long': 5,
               'maxW': 6,
               'minP': 7,
               'ne34': 8,
               'se34': 9,
               'sw34': 10,
               'nw34': 11,
               'ne50': 12,
               'se50': 13,
               'sw50': 14,
               'nw50': 15,
               'ne64': 16,
               'se64': 17,
               'sw64': 18,
               'nw64': 19,
               }

    def proc_lat_long(self, coord_str):
        float_val = float(coord_str[:-1])
        if coord_str[-1] == 'W':
            return -float_val
        if coord_str[-1] == 'S':
            return -float_val
        return float_val

    def __init__(self, rec_str):
        col_idx = self.__class__.col_idx
        field_list = [s.strip() for s in rec_str.split(',')]
        time_str = field_list[col_idx['date']] + field_list[col_idx['time']]
        format_str = '%Y%m%d%H%M'
        try:
            self.record_time = datetime.datetime.strptime(time_str, format_str)
        except ValueError as ve:
            raise ValueError("Error parsing record date, {}".format(time_str))

        self.identifier = field_list[col_idx['ident']]
        self.system_status = field_list[col_idx['status']]
        self.latitude = self.proc_lat_long(field_list[col_idx['lat']])
        self.longitude = self.proc_lat_long(field_list[col_idx['long']])
        self.max_sustained_wind = float(field_list[col_idx['maxW']])
        self.min_pressure = float(field_list[col_idx['minP']])
        self.ne34_radius = float(field_list[col_idx['ne34']])
        self.se34_radius = float(field_list[col_idx['se34']])
        self.sw34_radius = float(field_list[col_idx['sw34']])
        self.nw34_radius = float(field_list[col_idx['nw34']])
        self.nw50_radius = float(field_list[col_idx['nw50']])
        self.ne50_radius = float(field_list[col_idx['ne50']])
        self.se50_radius = float(field_list[col_idx['se50']])
        self.sw50_radius = float(field_list[col_idx['sw50']])
        self.nw50_radius = float(field_list[col_idx['nw50']])
        self.ne64_radius = float(field_list[col_idx['ne64']])
        self.se64_radius = float(field_list[col_idx['se64']])
        self.sw64_radius = float(field_list[col_idx['sw64']])
        self.nw64_radius = float(field_list[col_idx['nw64']])

    def set_track_start_time(self, track_start: datetime.datetime):
        self._track_start = track_start
        delta: datetime.timedelta = (self.record_time - self._track_start)
        self.minutes = delta.days * 24.0 * 60.0 + delta.seconds / 60.0

    @property
    def coords(self):
        return (self.longitude, self.latitude, self.max_sustained_wind,
                self.minutes)


if __name__ == '__main__':
    a_rec_str = '20160902, 0530, L, HU, 30.1N,  84.1W,  70,  981,  130,  15'\
                '0,   60,   60,   70,  110,   40,   30,   30,   '\
                '40,   30,    0,'

    a_record = HurricaneRecord(a_rec_str)
    expected_time = datetime.datetime(2016, 9, 2, 5, 30)
    tdelta_float = (expected_time - a_record.record_time) / \
                   datetime.timedelta(days=1)
    import math
    assert math.isclose(0.0, tdelta_float, abs_tol=1e-7), "Time parsed " \
                                                          "incorrectly."

    assert math.isclose(30.1, a_record.latitude, abs_tol=0.01)
    assert math.isclose(-84.1, a_record.longitude, abs_tol=0.01)

    assert math.isclose(30.0, a_record.sw64_radius, abs_tol=1.0)

    print("All __main__ tests passed.")

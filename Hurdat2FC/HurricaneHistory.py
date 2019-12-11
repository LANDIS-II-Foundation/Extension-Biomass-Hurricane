'''
Based on the National Hurricane Center's hurricane database format for a
specific date/time/location. This module is part of the Hurdat2FC repository
by Paul Schrum.

A single instance of HurricaneHistory is intended. That single instance
contains all hurricane tracks in the database being read.
'''

__author__ = 'Paul Schrum'

from HurricaneTrack import HurricaneTrack

class HurricaneHistory:
    def __init__(self, file_to_read):
        self.all_storms = {}
        with open(file_to_read, 'r') as f:
            all_recs_list = f.readlines()

        for idx, rec in enumerate(all_recs_list):
            record = [s.strip() for s in rec.split(',')]
            rec_len = len(record)
            if rec_len == 4:
                track_rec_count = int(record[2])
                last_record = idx + track_rec_count + 1
                thisHurRecs = all_recs_list[idx:last_record]
                one_track = HurricaneTrack(thisHurRecs)
                repr = one_track.__repr__()
                self.all_storms[repr] = one_track

if __name__ == '__main__':
    test_db_file = "Test Data/hurdat2-2016-2018.txt"
    real_db_file = r"D:\Research\Datasets\Weather\Hurricanes\hurdat" \
                   r"2-1851-2018-051019.txt"

    import os
    if not os.path.exists(test_db_file):
        print("Can't find testing file; can't run tests.")
        import sys
        sys.exit(0)

    hist = HurricaneHistory(real_db_file)
    assert hist
    assert len(hist.all_storms) == 50


    print("All __main__ tests passed.")


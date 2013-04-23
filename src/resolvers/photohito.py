# -*- coding: utf-8 -*-

from resolvers import *

class Photohito(OpenGraphResolver):
    @property
    def service_name(self):
        return "PHOTOHITO"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?photohito\.com/photo/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "photohito"
        columns = ["id", "prefix"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        uri = self.read_og("http://photohito.com/photo/%s/" % param)
        if not uri:
            raise PictureNotFoundError()

        prefix = uri[:- len("_s.jpg")]

        self.insert_all(cursor, table, (param, prefix))
        return prefix

    def get_full(self, match):
        return self.work(match) + "_o.jpg"

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return self.work(match) + "_m.jpg"

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://", 1)

    def get_thumb(self, match):
        return self.work(match) + "_s.jpg"

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

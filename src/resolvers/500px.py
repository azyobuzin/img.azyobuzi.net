# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *
from resolvers.private_constant import *

class _500px(StoringResolver):
    @property
    def service_name(self):
        return "500px"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?500px\.com/photo/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "500px"
        columns = ["id", "image5"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            response = urllib2.urlopen("https://api.500px.com/v1/photos/%s?image_size=5&consumer_key=%s" % (param, consumer_key_500px))
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        image5 = json.load(response)["photo"]["image_url"]

        self.insert_all(cursor, table, (param, image5))
        return image5

    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return self.work(match)[:-len("5.jpg")] + "4.jpg"

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://", 1)

    def get_thumb(self, match):
        return self.work(match)[:-len("5.jpg")] + "2.jpg"

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

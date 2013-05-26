# -*- coding: utf-8 -*-

import urllib2
from xml.etree import ElementTree

from resolvers import *
from resolvers.private_constant import *

class Tinami(StoringResolver):
    @property
    def service_name(self):
        return "TINAMI"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?tinami\.(?:com|jp)/(?:view/(\d+)/?|([a-z0-9]+))(?:\?.*)?$"
    
    def get_parameters(self, match):
        param = match.group(1)
        return param if param else str(int(match.group(2), base=36))

    def _work(self, param, cursor):
        table = "tinami"
        columns = ["cont_id", "thumbnail", "image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        response = urllib2.urlopen("http://api.tinami.com/content/info?api_key=%s&cont_id=%s"
            % (tinami_api_key, param))

        root = ElementTree.fromstring(response.read())
        content = root.find("content")

        if not content:
            raise PictureNotFoundError()

        thumbnail = list(content.find("thumbnails"))[0].get("url")

        images = content.find("images")
        image = (images if images else content).find("image").find("url").text

        self.insert_all(cursor, table, (param, thumbnail, image))
        return dict(zip(columns[1:], (thumbnail, image)))

    def get_full(self, match):
        return self.work(match)["image"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://api.tinami.com", "https://www.tinami.com/api", 1)

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.work(match)["thumbnail"]

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

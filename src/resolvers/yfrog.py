# -*- coding: utf-8 -*-

import urllib2
from xml.etree import ElementTree

from resolvers import *

class Yfrog(StoringResolver):
    @property
    def service_name(self):
        return "yfrog"

    @property
    def regex_str(self):
       return r"^https?://(?:www\.|twitter\.)?yfrog\.com/(\w+)(?::\w+)?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "yfrog"
        columns = ["hash", "image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            response = urllib2.urlopen("http://yfrog.com/api/xmlInfo?path=" + param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        root = ElementTree.fromstring(response.read())
        tag = root.tag
        ns = tag[: tag.index("}") + 1]

        image = root.find(ns + "links").find(ns + "image_link").text

        self.insert_all(cursor, table, (param, image))
        return image

    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return "http://yfrog.com/%s:iphone" % match.group(1)

    def get_large_https(self, match):
        return "https://yfrog.com/%s:iphone" % match.group(1)

    def get_thumb(self, match):
        return "http://yfrog.com/%s:small" % match.group(1)

    def get_thumb_https(self, match):
        return "https://yfrog.com/%s:small" % match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

# -*- coding: utf-8 -*-

import urllib2
from xml.etree import ElementTree

from resolvers import *

class Photozou(StoringResolver):
    @property
    def service_name(self):
        return u"フォト蔵"
    
    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?photozou\.jp/photo/(?:show|photo_only)/\d+/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "photozou"
        columns = ["id", "image", "original_image", "thumbnail_image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        try:
            response = urllib2.urlopen("http://api.photozou.jp/rest/photo_info?photo_id=" + param)
        except urllib2.HTTPError as e:
            if e.code == 400:
                raise PictureNotFoundError()
            else:
                raise e

        root = ElementTree.fromstring(response.read())

        photo = root.find("info").find("photo")

        image = photo.find("image_url").text
        thumbnail_image = photo.find("thumbnail_image_url").text

        original_image_tag = photo.find("original_image_url")
        original_image = original_image_tag.text if original_image_tag is not None else image

        self.insert_all(cursor, table, (param, image, original_image, thumbnail_image))
        return dict(zip(columns[1:], (image, original_image, thumbnail_image)))
    
    def get_full(self, match):
        return self.work(match)["original_image"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)
    
    def get_large(self, match):
        return self.work(match)["image"]

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://", 1)
    
    def get_thumb(self, match):
        return self.work(match)["thumbnail_image"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

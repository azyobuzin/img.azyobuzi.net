# -*- coding: utf-8 -*-

import urllib2
from xml.etree import ElementTree

from resolvers import *
from resolvers.private_constant import *

class Flickr(StoringResolver):
    @property
    def service_name(self):
        return "Flickr"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?(?:flickr\.com/photos/(?:[\w\-_@]+)/(\d+)(?:/in/[\w\-]*)?|flic\.kr/p/(\w+))/?(?:\?.*)?$"

    @staticmethod
    def b58decode(s): #http://www.flickr.com/groups/api/discuss/72157616713786392/72157621745921901/
        alphabet = '123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ'
        num = len(s)
        decoded = 0 ;
        multi = 1;
        for i in reversed(range(0, num)):
            decoded = decoded + multi * (alphabet.index(s[i]))
            multi = multi * len(alphabet)
        return decoded;

    def get_parameters(self, match):
        return {"id": match.group(1), "shorten": match.group(2)}

    def _work(self, param, cursor):
        if not param["id"]:
            param = dict(param) #キャッシュ漏れを防ぐ
            param["id"] = self.b58decode(param["shorten"])

        table = "flickr"
        columns = ["id", "thumbnail", "medium", "original"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param["id"]})
        if result:
            return dict(zip(columns[1:], result))

        response = urllib2.urlopen("http://www.flickr.com/services/rest?method=flickr.photos.getSizes&api_key=%s&photo_id=%s"
            % (flickr_api_key, param["id"]))

        root = ElementTree.fromstring(response.read())
        sizes = root.find("sizes")

        if sizes is None:
            raise PictureNotFoundError()

        thumbnail = None
        medium = None
        large = None
        original = None

        for elm in sizes:
            label = elm.get("label")

            if label == "Thumbnail":
                thumbnail = elm.get("source")
            elif label == "Medium":
                medium = elm.get("source")
            elif label == "Large":
                large = elm.get("source")
            elif label == "Original":
                original = elm.get("source")

        if large is None:
            large = medium

        if original is None:
            original = large

        self.insert_all(cursor, table, (param["id"], thumbnail, medium, original))
        return dict(zip(columns[1:], (thumbnail, medium, original)))

    def get_full(self, match):
        return self.work(match)["original"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return self.work(match)["medium"].replace("https://", "http://", 1)

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://", 1)

    def get_thumb(self, match):
        return self.work(match)["thumbnail"].replace("https://", "http://", 1)

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

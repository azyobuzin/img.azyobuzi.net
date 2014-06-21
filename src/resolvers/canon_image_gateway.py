# -*- coding: utf-8 -*-

from HTMLParser import HTMLParser
import urllib2

from resolvers import *

class CigParser(HTMLParser):
    def __init__(self):
        HTMLParser.__init__(self)
        self.image = None
        self.original = None

    def handle_starttag(self, tag, attrs):
        dic = dict(attrs)
        if tag == "meta" and dic.get("property") == "og:image":
            self.image = dic["content"]
        elif tag == "a" and dic.get("id") == "jsOriginalImageLink":
            self.original = dic["href"]

class CanonImageGateway(StoringResolver):
    @property
    def service_name(self):
        return "CANON iMAGE GATEWAY"

    @property
    def regex_str(self):
        return r"^https?://opa\.cig2\.imagegateway\.net/s/(?:t/)?(\w+(?:/\w+)?)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "canon_image_gateway"
        columns = ["id", "image", "original"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        try:
            html = urllib2.urlopen("http://opa.cig2.imagegateway.net/s/" + param).read().decode("utf-8")
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        parser = CigParser()
        parser.feed(html)

        image = parser.image
        original = parser.original

        if not image or not original:
            raise PictureNotFoundError()

        self.insert_all(cursor, table, (param, image, original))
        return dict(zip(columns[1:], (image, original)))

    def get_full(self, match):
        return self.work(match)["original"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.work(match)["image"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

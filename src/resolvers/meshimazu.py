# -*- coding: utf-8 -*-

from HTMLParser import HTMLParser
import urllib2

from resolvers import *

class MeshimazuParser(HTMLParser):
    def __init__(self):
        HTMLParser.__init__(self)
        self.image = None

    def handle_starttag(self, tag, attrs):
        dic = dict(attrs)
        if tag == "img" and dic.get("class") == "testes":
            self.image = dic["src"]

class Meshimazu(StoringResolver):
    @property
    def service_name(self):
        return u"メシマズ"

    @property
    def regex_str(self):
        return r"^http://(?:www\.)?meshimazu\.net/posts/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "meshimazu"
        columns = ["id", "image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            response = urllib2.urlopen("http://www.meshimazu.net/posts/%s/" % param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        parser = MeshimazuParser()
        parser.feed(response.read().decode("utf-8"))
        parser.close()

        self.insert_all(cursor, table, (param, parser.image))
        return parser.image

    def get_full(self, match):
        return "http://www.meshimazu.net" + self.work(match)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return self.get_full(match)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

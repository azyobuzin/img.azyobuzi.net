# -*- coding: utf-8 -*-

from sgmllib import SGMLParser
import urllib2

from resolvers import *

class HatenaFotolifeParser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.foto_src = None
        self.original_link = None

    def do_img(self, attributes):
        dic = dict(attributes)
        if "id" in dic and dic["id"].startswith("foto-for-html-tag-"):
            self.foto_src = dic["src"]

    def start_a(self, attributes):
        dic = dict(attributes)
        if "href" in dic and dic["href"].find("_original.") != -1:
            self.original_link = dic["href"]

class HatenaFotolife(StoringResolver):
    @property
    def service_name(self):
        return u"はてなフォトライフ"

    @property
    def regex_str(self):
        return r"^http://f\.hatena\.ne\.jp/(\w+)/(\d+)(?:\?.*)?$"

    def get_parameters(self, match):
        return {"username": match.group(1), "id": match.group(2)}

    def _work(self, param, cursor):
        table = "hatena_fotolife"
        columns = ["username", "id", "full", "large", "thumb"]
        result = self.select_one(cursor, table, columns[2:], {columns[0]: param["username"], columns[1]: param["id"]})
        if result:
            return dict(zip(columns[2:], result))

        req_uri = "http://f.hatena.ne.jp/%(username)s/%(id)s" % param

        try:
            response = urllib2.urlopen(req_uri)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        if response.geturl() != req_uri: #リダイレクトされたと判断
            raise PictureNotFoundError()

        html = response.read().decode("utf-8")

        if """<img src="" alt="" title="" width="" height="" class="foto" style="" />""" in html:
            raise PictureNotFoundError()

        parser = HatenaFotolifeParser()
        parser.feed(html)
        parser.close()

        large_size = parser.foto_src

        if """<object data="/tools/flvplayer.swf" type="application/x-shockwave-flash\"""" not in html:
            #画像
            if parser.original_link:
                original_size = parser.original_link
            else:
                original_size = large_size
        else:
            #動画
            original_size = "http://cdn-ak.f.st-hatena.com/images/fotolife/%s/%s/%s/%s.flv" \
                % (param["username"][0], param["username"], param["id"][0:8], param["id"])

        thumbnail = "http://cdn-ak.f.st-hatena.com/images/fotolife/%s/%s/%s/%s_120.jpg" \
            % (param["username"][0], param["username"], param["id"][0:8], param["id"])

        self.insert_all(cursor, table, (param["username"], param["id"], original_size, large_size, thumbnail))
        return dict(zip(columns[2:], (original_size, large_size, thumbnail)))

    def get_full(self, match):
        return self.work(match)["full"].replace(".flv", ".jpg")

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.work(match)["large"]

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return self.work(match)["thumb"]

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        result = self.work(match)["full"]
        return result if ".flv" in result else None

    def get_video_https(self, match):
        return None

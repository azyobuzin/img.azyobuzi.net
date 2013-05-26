# -*- coding: utf-8 -*-

from sgmllib import SGMLParser
import urllib2

from resolvers import *

class OwlyParser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.original = None
        self.image_wrapper_flag = False

    def start_div(self, attributes):
        if dict(attributes).get("class") == "imageWrapper":
            self.image_wrapper_flag = True

    def end_div(self):
        self.image_wrapper_flag = False

    def do_img(self, attributes):
        if self.image_wrapper_flag:
            self.original = dict(attributes)["src"]

class Owly(StoringResolver):
    @property
    def service_name(self):
        return "Ow.ly"
    
    @property
    def regex_str(self):
       return r"^http://(?:www\.)?ow\.ly/i/(\w+)(?:/(?:original/?)?)?(?:\?.*)?$"
        
    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "owly"
        columns = ["id", "original"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            response = urllib2.urlopen("http://ow.ly/i/%s/original" % param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        html = response.read().decode("utf-8")
        
        parser = OwlyParser()
        parser.feed(html)
        parser.close()           
        original = parser.original

        self.insert_all(cursor, table, (param, original))
        return original
    
    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)
    
    def get_large(self, match):
        return "http://static.ow.ly/photos/normal/%s.jpg" % match.group(1)

    def get_large_https(self, match):
        return "https://static.ow.ly/photos/normal/%s.jpg" % match.group(1)
    
    def get_thumb(self, match):
        return "http://static.ow.ly/photos/thumb/%s.jpg" % match.group(1)

    def get_thumb_https(self, match):
        return self.get_full_https(match)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

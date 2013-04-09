# -*- coding: utf-8 -*-

import urllib2

from resolvers import *

class Dropbox(StoringResolver):
    @property
    def service_name(self):
        return "Dropbox"

    @property
    def regex_str(self):
        return r"^https?://(?:(?:www\.|dl\.)?dropbox\.com/s/(\w+)/([\w\-\.%]+\.(?:jpeg?|jpg|png|gif|bmp|dib|tiff?))|(?:www\.)?db\.tt/(\w+))/?(?:\?.*)?$"

    def get_parameters(self, match):
        return {"token": match.group(1), "filename": match.group(2), "shorten": match.group(3)}

    def _work(self, param, cursor):
        if param["shorten"]:
            table = "dropbox"
            columns = ["shorten", "expanded"]
            result = self.select_one(cursor, table, columns[1:], {columns[0]: param["shorten"]})
            if result:
                param = self.get_parameters(self.regex.match(result[0]))
            else:
                try:
                    response = urllib2.build_opener(DontRedirectHandler).open(
                        Request2("http://db.tt/" + param["shorten"], method="HEAD")
                    )
                except urllib2.HTTPError as e:
                    if e.code == 404:
                        raise PictureNotFoundError()
                    else:
                        raise e

                expanded = response.headers["location"]
                self.insert_all(cursor, table, (param["shorten"], expanded))
                param = self.get_parameters(self.regex.match(expanded))

        return "https://dl.dropbox.com/s/%(token)s/%(filename)s" % param

    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return self.get_full(match).replace("https://", "http://", 1)

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.get_full(match)

    def get_thumb_https(self, match):
        return self.get_full_https(match)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

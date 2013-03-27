# -*- coding: utf-8 -*-

import abc
import re

class Resolver(object):
    __metaclass__ = abc.ABCMeta

    def __init__(self):
        self._regex = None

    @abc.abstractproperty
    def service_name(self):
        return "img.azyobuzi.net"

    @abc.abstractproperty
    def regex_str(self):
        return None

    @property
    def regex(self):
        if self._regex is None:
            self._regex = re.compile(self.regex_str, re.IGNORECASE)
        return self._regex

    @abc.abstractmethod
    def get_full(self, match):
        return None

    @abc.abstractmethod
    def get_full_https(self, match):
        return None

    @abc.abstractmethod
    def get_large(self, match):
        return None

    @abc.abstractmethod
    def get_large_https(self, match):
        return None

    @abc.abstractmethod
    def get_thumb(self, match):
        return None

    @abc.abstractmethod
    def get_thumb_https(self, match):
        return None

    @abc.abstractmethod
    def get_video(self, match):
        return None

    @abc.abstractmethod
    def get_video_https(self, match):
        return None

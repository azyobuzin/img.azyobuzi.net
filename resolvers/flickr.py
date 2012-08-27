# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import xml.etree.ElementTree as ET
from private_constant import *

class flickr:
	def __str__(self):
		return "Flickr"
	
	regexStr = "^https?://(?:www\\.)?(?:flickr\\.com/photos/(?:[\\w\\-_@]+)/(\\d+)|flic\\.kr/p/(\\w+))/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def b58decode(self, s): #http://www.flickr.com/groups/api/discuss/72157616713786392/72157621745921901/
		alphabet = '123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ'
		num = len(s)
		decoded = 0 ;
		multi = 1;
		for i in reversed(range(0, num)):
			decoded = decoded + multi * ( alphabet.index( s[i] ) )
			multi = multi * len(alphabet)
		return decoded;
	
	def getUriData(self, match):
		id = match.group(1)
		
		if id is None:
			id = self.b58decode(match.group(2))
		
		if self.last is not None and str(self.last[0]) == str(id):
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM flickr WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = urllib2.urlopen("http://www.flickr.com/services/rest?method=flickr.photos.getSizes&api_key=%s&photo_id=%s"
				% (flickrApiKey, id))
			
			root = ET.fromstring(httpRes.read())
			sizes = root.find("sizes")
			
			if sizes is None:
				return None
			
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
			
			c.execute("INSERT INTO flickr VALUES (%s, %s, %s, %s)"
				% (db.literal(id), db.literal(thumbnail), db.literal(medium), db.literal(original))
			)
			
			db.commit()
			
			self.last = [id, thumbnail, medium, original]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None

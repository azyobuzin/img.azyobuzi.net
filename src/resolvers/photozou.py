# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import xml.etree.ElementTree as ET
from private_constant import *

class photozou:
	def __str__(self):
		return u"フォト蔵"
	
	regexStr = "^https?://(?:www\\.)?photozou\\.jp/photo/(?:show|photo_only)/\\d+/(\\d+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and str(self.last[0]) == str(id):
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM photozou WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://api.photozou.jp/rest/photo_info?photo_id=" + id)
			except:
				return None
			
			root = ET.fromstring(httpRes.read())
			
			photo = root.find("info").find("photo")
			
			image = photo.find("image_url").text
			thumbnail_image = photo.find("thumbnail_image_url").text
			
			original_image_tag = photo.find("original_image_url")
			original_image = original_image_tag.text if original_image_tag is not None else image
			
			c.execute("INSERT INTO photozou VALUES (%s, %s, %s, %s)"
				% (db.literal(id), db.literal(image), db.literal(original_image), db.literal(thumbnail_image))
			)
			
			db.commit()
			
			self.last = [id, image, original_image, thumbnail_image]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None

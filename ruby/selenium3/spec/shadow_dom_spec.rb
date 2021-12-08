# frozen_string_literal: true
require 'spec_helper'

RSpec.describe 'Shadow DOM' do
  context 'when old Chrome' do
    #  What most people use for Shadow DOM Elements;
    #  Still works for Chrome < v96, Edge < v96, Safari
    it "old code works" do
      user = ENV['SAUCE_USERNAME']
      key = ENV['SAUCE_ACCESS_KEY']

      caps = Selenium::WebDriver::Remote::W3C::Capabilities.new(browser_name: 'chrome',
                                                                browser_version: '95.0',
                                                                'sauce:options': {username: user,
                                                                                  access_key: key})

      @driver = Selenium::WebDriver.for :remote,
                                        url: 'https://ondemand.saucelabs.com/wd/hub/',
                                        desired_capabilities: caps

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      shadow_root = @driver.execute_script('return arguments[0].shadowRoot', shadow_host)
      shadow_content = shadow_root.find_element(css: '#shadow_content')

      expect(shadow_content.text).to eq 'some text'
    end
  end

  context 'when new Chrome' do
    # Same code as above, but with Chromium 96+, it raises a NoMethodError
    it 'old code broken' do
      @driver = Selenium::WebDriver.for :chrome

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      shadow_root = @driver.execute_script('return arguments[0].shadowRoot', shadow_host)

      expect {
        shadow_root.find_element(css: '#shadow_content')
      }.to raise_error(NoMethodError)
    end

    # If you absolutely *have to use Shadow Dom Elements in latest Chromium versions
    # in Selenium 3, this is the way to do it. It is hacky, so please update to Selenium 4 as soon as you can.
    it 'hack works' do
      @driver = Selenium::WebDriver.for :chrome

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')

      shadow_root_hash = @driver.execute_script('return arguments[0].shadowRoot', shadow_host)
      shadow_root_id = shadow_root_hash['shadow-6066-11e4-a52e-4f735466cecf']
      shadow_root = Selenium::WebDriver::Element.new(@driver.send(:bridge), shadow_root_id)

      shadow_content = shadow_root.find_element(css: '#shadow_content')

      expect(shadow_content.text).to eq 'some text'
    end
  end

  context 'when Firefox' do
    # Firefox is special
    it 'workaround' do
      @driver = Selenium::WebDriver.for :firefox

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      children = @driver.execute_script('return arguments[0].shadowRoot.children', shadow_host)

      shadow_content = children.first { |child| child.attribute('id') == 'shadow_content' }

      expect(shadow_content.text).to eq 'some text'
    end
  end
end

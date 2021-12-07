# frozen_string_literal: true
require 'spec_helper'

RSpec.describe 'Shadow DOM' do
  context 'when Chrome 95' do
    it "old way works" do
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

  context 'when Chrome 96' do
    it 'old way does not work' do
      @driver = Selenium::WebDriver.for :chrome

      @driver.get('http://watir.com/examples/shadow_dom.html')
      shadow_host = @driver.find_element(css: '#shadow_host')
      shadow_root = @driver.execute_script('return arguments[0].shadowRoot', shadow_host)

      expect {
        shadow_root.find_element(css: '#shadow_content')
      }.to raise_error(NoMethodError)
    end

    it 'hack to make it work' do
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
    it 'uses children' do
      @driver = Selenium::WebDriver.for :firefox

      @driver.get('http://watir.com/examples/shadow_dom.html')
      shadow_host = @driver.find_element(css: '#shadow_host')
      children = @driver.execute_script('return arguments[0].shadowRoot.children', shadow_host)

      shadow_content = children.first { |child| child.attribute('id') == 'shadow_content' }

      expect(shadow_content.text).to eq 'some text'
    end
  end
end

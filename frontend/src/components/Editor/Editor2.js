import React, { Component } from 'react'
import { EditorState } from 'draft-js'
import Editor from 'draft-js-plugins-editor'
import createMentionPlugin, { defaultSuggestionsFilter } from 'draft-js-mention-plugin'
import { convertToRaw } from 'draft-js'
import _ from 'lodash'
import pluginStyles from '../../../node_modules/draft-js-mention-plugin/lib/plugin.css'
import { stateToHTML } from 'draft-js-export-html'

export default class SimpleMentionEditor extends Component {
  constructor(props) {
    super(props)

    this.mentionPlugin = createMentionPlugin()
  }

  static getDerivedStateFromProps(props, state) {
    if (props.value && !state.updated) {
      return {
        updated: true,
        editorState: EditorState.createWithContent(props.value)
      }
    }
    return null
  }

  state = {
    updated: false,
    editorState: this.props.value ? EditorState.createWithContent(this.props.value) : EditorState.createEmpty(),
    suggestions: this.props.mentions
  }

  onChange = (editorState) => {
    this.setState({
      editorState
    })
  }

  onSearchChange = ({ value }) => {
    this.setState({
      suggestions: defaultSuggestionsFilter(value, this.props.mentions)
    })
  }

  onAddMention = () => {
    // get the mention object selected
  }

  focus = () => {
    this.editor.focus()
  }

  renderContentAsRawJs() {
    const contentState = this.state.editorState.getCurrentContent()
    const raw = convertToRaw(contentState)

    return JSON.stringify(raw, null, 2)
  }

  toHtml = () => {
    const contentState = this.state.editorState.getCurrentContent()
    const options = {
      // eslint-disable-next-line consistent-return
      entityStyleFn: (entity) => {
        const entityType = entity.get('type').toLowerCase()
        if (entityType === 'mention') {
          const data = entity.getData()
          return {
            element: 'span',
            attributes: {
              'data-mention-id': _.get(data, 'mention.id'),
              class: 'mention_class'
            },
            style: {
              // Put styles here...
            }
          }
        }
      }
    }
    return stateToHTML(contentState, options)
  }

  render() {
    const { MentionSuggestions } = this.mentionPlugin
    const plugins = [this.mentionPlugin]
    const { className, style, placeholder } = this.props

    return (
      <div onClick={this.focus}>
        <div className={`editor ${className || ''}`} style={style}>
          <Editor
            editorState={this.state.editorState}
            onChange={this.onChange}
            onBlur={() => {
              if (this.props.onChange) this.props.onChange(this.state.editorState.getCurrentContent(), this.toHtml())
            }}
            placeholder={placeholder}
            plugins={plugins}
            ref={(element) => {
              this.editor = element
            }}
          />
          <MentionSuggestions
            onSearchChange={this.onSearchChange}
            suggestions={this.state.suggestions}
            onAddMention={this.onAddMention}
          />
        </div>
      </div>
    )
  }
}

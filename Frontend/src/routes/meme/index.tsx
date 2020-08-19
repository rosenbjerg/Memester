import { Component, h } from "preact";
import { Meme } from "../../models";
import Loading from "../../components/Loading";
import ky from "ky";
import * as style from './style.css'

interface Props {
    threadId: number;
    memeId: number;
}

interface State {
    meme?: Meme;
}

export default class MemePage extends Component<Props, State> {
    componentDidMount() {
        ky.get(`/api/threads/${this.props.threadId}/${this.props.memeId}`)
            .json<Meme>()
            .then(meme => this.setState({ meme }));
    }

    render({ memeId, threadId }: Props, { meme }: State) {
        if (!meme) return <Loading />;

        return (
            <div class={style.wrapper}>
                <div class={style.memeDiv}>
                    <div class={style.memeHeader}>
                        <div>{meme.name}</div>
                        <div class={style.headerControls}>
                            <div class={`${style.nextButton} material-icons`}>navigate_next</div>
                        </div>
                    </div>
                    <video src={`/api/${threadId}/${memeId}/video`} />
                    <div class={style.memeFooter}></div>
                </div>
            </div>
        );
    }
}
